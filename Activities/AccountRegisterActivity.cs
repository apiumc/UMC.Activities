using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class AccountRegisterActivity : WebActivity
    {

        void SendMobileCode(string mobile)
        {

            var user = UMC.Security.Identity.Current;

            var req = this.Context.Request;

            var hask = new Hashtable();

            var session = new UMC.Data.Session<Hashtable>(mobile);
            if (session.ModifiedTime.AddMinutes(15) > DateTime.Now)
            {
                hask = session.Value;
            }
            else
            {
                hask["Code"] = UMC.Data.Utility.NumberCode(Guid.NewGuid().GetHashCode(), 6);
            }
            var times = UMC.Data.Utility.IntParse(String.Format("{0}", hask["Times"]), 0) + 1;
            if (times > 5)
            {
                var date = session.ModifiedTime;
                if (date.AddHours(3) > DateTime.Now)
                {
                    this.Prompt("您已经超过了5次，请您三小时后再试");
                }
                else
                {
                    times = 0;
                }
            }
            session.Commit(hask, user);


            hask["DateTime"] = DateTime.Now;

            Net.Message.Instance().Send("Register", hask, mobile);

        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var user = Web.UIFormDialog.AsyncDialog("Register", d =>
            {
                if (request.SendValues != null && request.SendValues.Count > 0)
                {
                    return this.DialogValue(request.SendValues);
                }

                var u = new UMC.Data.Entities.User { Username = String.Empty };

                var dialog = new Web.UIFormDialog();
                dialog.Title = "账户注册";
                dialog.AddText("昵称", "Alias", u.Alias);

                dialog.AddText("手机号码", "Username", u.Username);
                dialog.AddVerify("验证码", "VerifyCode", "您收到的验证码").Put("For", "Username").Put("To", "Mobile")
                .Put("Command", request.Command).Put("Model", request.Model);

                if (request.IsApp == false)
                {
                    dialog.AddPassword("密码", "Password", false);
                    dialog.AddPassword("确认密码", "NewPassword2", false).Put("placeholder", "再输入一次密码").Put("ForName", "Password");
                }
                dialog.Submit("确认注册", request, "User");
                return dialog;

            });
            if (user.ContainsKey("Mobile"))
            {
                var mobile = user["Mobile"];
                var account = Data.DataFactory.Instance().Account(mobile, UMC.Security.Account.MOBILE_ACCOUNT_KEY);
                //Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Account>()
                //     .Where.And().Equal(new UMC.Data.Entities.Account
                //     {
                //         Name = mobile,
                //         Type = UMC.Security.Account.MOBILE_ACCOUNT_KEY
                //     }).Entities.Single();
                if (account != null)
                {
                    this.Prompt("此手机号码已经注册，你可直接登录");
                }
                this.SendMobileCode(mobile);
                this.Prompt("验证码已发送", false);
                this.Context.Send(new UMC.Web.WebMeta().UIEvent("VerifyCode", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("text", "验证码已发送")), true);
            }
            var username = user["Username"];
            if (user.ContainsKey("VerifyCode"))
            {
                var VerifyCode = user["VerifyCode"];
                var session = new UMC.Data.Session<Hashtable>(username);
                if (session.Value != null)
                {
                    var code = session.Value["Code"] as string;
                    if (String.Equals(code, VerifyCode) == false)
                    {
                        this.Prompt("请输入正确的验证码");
                    }
                }
                else
                {
                    this.Prompt("请输入正确的验证码");

                }
            }


            UMC.Data.Entities.Account ac = new UMC.Data.Entities.Account { Name = username };
            if (Data.Utility.IsEmail(username))
            {
                ac.Type = UMC.Security.Account.EMAIL_ACCOUNT_KEY;

            }
            else if (Data.Utility.IsPhone(username))
            {
                ac.Type = UMC.Security.Account.MOBILE_ACCOUNT_KEY;
            }
            if (ac.Type.HasValue == false)
            {
                this.Prompt("只支持手机号注册");
            }
            if (Data.DataFactory.Instance().Account(username, ac.Type.Value) != null)
            {
                switch (ac.Type.Value)
                {
                    case UMC.Security.Account.EMAIL_ACCOUNT_KEY:
                        this.Prompt("此邮箱已经注册");
                        break;
                    default:
                        this.Prompt("此手机号已经注册");
                        break;
                }
            }
            var passwork = user["Password"];
            var NewPassword2 = user["NewPassword2"];
            if (String.IsNullOrEmpty(NewPassword2) == false)
            {
                if (String.Equals(passwork, NewPassword2) == false)
                {
                    this.Prompt("两次密码不相同，请确认密码");
                }

            }
            var Alias = user["Alias"] ?? username;
            var uM = UMC.Security.Membership.Instance();
            var uid = uM.CreateUser(username, Alias);
            if (uid != Guid.Empty)
            {

                if (user.ContainsKey("VerifyCode"))
                {
                    UMC.Security.Account.Post(ac.Name, uid, UMC.Security.UserFlags.Normal, ac.Type.Value);
                }
                else
                {
                    UMC.Security.Account.Post(ac.Name, uid, UMC.Security.UserFlags.UnVerification, ac.Type.Value);
                }
                var iden = uM.Identity(username);

                UMC.Security.AccessToken.Login(iden, UMC.Security.AccessToken.Token.Value, request.IsApp ? "App" : "Client", true);

                if (String.IsNullOrEmpty(passwork) == false)
                {
                    uM.Password(username, username);
                }
                this.Context.Send(new UMC.Web.WebMeta().Put("type", "User"), false);
                this.Prompt("注册成功");
            }
            else
            {
                this.Prompt("已经存在这个用户");
            }


        }

    }
}