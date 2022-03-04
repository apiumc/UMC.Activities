using System;
using UMC.Security;
using UMC.Web;


namespace UMC.Activities
{
    /*
     * 后台登录，进入管理后台
     *
     * */

    [Mapping("Settings", "Login", Auth = WebAuthType.All, Desc = "后台登录")]
    class SettingsLoginActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            WebMeta user = this.AsyncDialog(d =>
            {

                WebMeta sendValue = request.SendValues ?? request.Arguments;
                if (sendValue.Count > 0)
                {
                    return this.DialogValue(sendValue);
                }
                UIFormDialog dialog = new UIFormDialog() { Title = "账户登录" };
                dialog.AddText("账户", "Username", "");
                dialog.AddPassword("账户密码", "Password", "");
                dialog.Submit("确认登录", request, "Cashier");
                return dialog;

            }, "Login");
            String username = user.Get("Username");
            String Password = user.Get("Password");

            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(Password))
            {
                this.Prompt("请输入用户名和密码");
            }


            int maxTimes = 5;
            Membership userManager = Membership.Instance();
            int times = userManager.Password(username, Password, maxTimes);

            switch (times)
            {
                case 0:
                    Identity iden = userManager.Identity(username);
                    System.Security.Principal.IPrincipal principal = iden;

                    if (principal.IsInRole(UMC.Security.Membership.UserRole) == false)
                    {
                        this.Prompt("您不是门店内部人员，不能从此登录。");
                    }
                    this.Context.Token.Login(iden,  request.IsApp ? 0 : 3600, request.IsApp ? "App" : "Desktop", true,request.UserHostAddress);
                    this.Context.OnReset();

                    this.Prompt("登录成功", false);

                    WebMeta print = new UMC.Web.WebMeta();
                    print["Alias"] = iden.Alias;
                    print["Src"] = Data.WebResource.Instance().ImageResolve(iden.Id.Value, "1", 5);
                    print["type"] = "Cashier";
                    this.Context.Send(print, true);
                    //this.Context.Send("Cashier", true);
                    break;
                case -4:
                    this.Prompt("您的账户已经禁用");
                    break;
                case -3:
                    this.Prompt("无此子账户");
                    break;
                case -2:
                    this.Prompt("您的用户已经锁定，请您联系管理员解锁");
                    break;
                case -1:
                    this.Prompt("您的用户不存在，请确定用户名", false);
                    break;
                default:
                    this.Prompt(String.Format("您的用户和密码不正确，您还有{0}次机会", maxTimes - times), false);


                    break;
            }
        }
    }
}