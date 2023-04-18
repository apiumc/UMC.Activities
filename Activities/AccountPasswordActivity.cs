using System;
using System.Linq;
using UMC.Security;
using UMC.Web;

namespace UMC.Activities
{
    class AccountPasswordActivity : WebActivity
    {


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            int type = UMC.Data.Utility.Parse(this.AsyncDialog("AccountType", "-1"), 0);
            var cUser = this.Context.Token.Identity();
            Guid user_id = UMC.Data.Utility.Guid(this.AsyncDialog("user_id", g =>
            {
                return Web.UIDialog.ReturnValue(cUser.Id.Value.ToString());
            })) ?? Guid.Empty;


            var user = Data.DataFactory.Instance().User(user_id);
            if (user == null)
            {
                this.Prompt("您是第三方应用账户，不支持修改密码");
                type = 0;
            }

            string VerifyCode = this.AsyncDialog("VerifyCode", "0");

            var Password = this.AsyncDialog("Password", d =>
            {
                if (request.SendValues != null)
                {
                    var meta = request.SendValues;
                    if (meta.ContainsKey("NewPassword"))
                    {
                        return Web.UIDialog.ReturnValue(meta);
                    }
                }
                var dialog = new Web.UIFormDialog();

                if (type > 0)
                {
                    dialog.Title = "找回密码";
                }
                else if (type < 0)
                {
                    dialog.Title = "修改密码";
                    if (cUser.IsAuthenticated == false)
                    {
                        this.Prompt("请登录");
                    }
                    dialog.AddPassword("原密码", "Password", true);//.Put("plo")
                }
                else
                {
                    if (cUser.IsAuthenticated == false)
                    {
                        this.Prompt("请登录");
                    }
                    dialog.Title = "设置密码";
                }
                dialog.AddPassword("新密码", "NewPassword", false);
                dialog.AddPassword("确认新密码", "NewPassword2", false).Put("ForName", "NewPassword");
                dialog.Submit("确认修改", $"{request.Model}.{request.Command}");
                return dialog;

            });
            var mc = UMC.Security.Membership.Instance();
            if (Password.ContainsKey("Password"))
            {

                if (mc.Password(cUser.Name, Password["Password"], 0) == 0)
                {
                    mc.Password(cUser.Name, Password["NewPassword"]);
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);

                    this.Context.Send($"{request.Model}.{request.Command}", true);

                }
                else
                {
                    this.Prompt("您的原密码不正确");
                }
            }
            else
            {

                if (user == null && cUser.Id == user_id)
                {
                    mc.CreateUser(cUser.Id.Value, cUser.Name, cUser.Alias);

                    mc.Password(cUser.Name, Password["NewPassword"]);
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);
                    this.Context.Send($"{request.Model}.{request.Command}", true);
                }


                var eac = Data.DataFactory.Instance().Account(user_id).Where(t => t.Type == type).First();
                var acc = Account.Create(eac);
                if (String.Equals(acc.Items[Account.KEY_VERIFY_FIELD] as string, VerifyCode))
                {
                    mc.Password(user.Username, Password["NewPassword"]);
                    acc.Items.Clear();
                    acc.Commit();
                    this.Prompt("密码修改成功，您可以用新密码登录了", false);
                    this.Context.Send($"{request.Model}.{request.Command}", true);
                }
                else
                {
                    this.Prompt("非法入侵");
                }
            }

        }

    }
}