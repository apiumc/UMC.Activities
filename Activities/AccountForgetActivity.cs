using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Security;
using UMC.Web;
using UMC.Data;

namespace UMC.Activities
{
    class AccountForgetActivity : WebActivity
    {
        bool Send(string mobile)
        {
            var acc = Data.DataFactory.Instance().Account(mobile);
            if (acc != null)
            {
                var ac = Account.Create(acc);
                var user = Data.DataFactory.Instance().User(acc.user_id.Value);
                var hask = new Hashtable();
                switch (acc.Type)
                {
                    case Account.MOBILE_ACCOUNT_KEY:
                        var times = UMC.Data.Utility.IntParse(String.Format("{0}", ac.Items["Times"]), 0) + 1;
                        if (times > 5)
                        {
                            var date = UMC.Data.Utility.TimeSpan(Data.Utility.IntParse(String.Format("{0}", ac.Items["Date"]), UMC.Data.Utility.TimeSpan()));
                            if (date.AddHours(3) > DateTime.Now)
                            {
                                this.Prompt("您已经超过了5次，请您三小时后再试");
                            }
                            else
                            {
                                times = 0;
                            }
                        }
                        ac.Items["Date"] = UMC.Data.Utility.TimeSpan().ToString();
                        ac.Items["UserHostAddress"] = this.Context.Request.UserHostAddress;
                        hask["Code"] = ac.Items[Account.KEY_VERIFY_FIELD] = Utility.NumberCode(Guid.NewGuid().GetHashCode(), 6);
                        ac.Commit();

                        Net.Message.Instance().Send("Forget", hask, ac.Name);

                        return true;
                    case Account.EMAIL_ACCOUNT_KEY:

                        var provider = UMC.Data.Reflection.GetDataProvider("account", "Forget");
                        if (provider != null)
                        {
                            hask["Code"] = ac.Items[Account.KEY_VERIFY_FIELD] = Utility.NumberCode(Guid.NewGuid().GetHashCode(), 6);
                            ac.Commit();
                            UMC.Data.Utility.AppendDictionary(user, hask);

                            hask["DateTime"] = DateTime.Now;
                            var mail = new System.Net.Mail.MailMessage();
                            mail.To.Add(mobile);
                            mail.Subject = Utility.Format(provider["Subject"], hask);
                            mail.Body = Utility.Format(provider["Body"], hask);
                            mail.IsBodyHtml = true;
                            UMC.Net.Message.Instance().Send(mail);
                        }
                        else
                        {
                            this.Prompt("未配置邮箱找回密码account.Forget");
                        }
                        return true;
                }
            }

            return false;
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var username = Web.UIDialog.AsyncDialog(this.Context, "Username", d =>
             {
                 var fd = new UMC.Web.UIFormDialog();
                 fd.Title = "找回密码";
                 fd.AddText("", "Username").Put("placeholder", "手机号码或邮箱");

                 fd.Submit("下一步",  "Forget");
                 return fd;
             });
            var type = 0;
            if (Data.Utility.IsEmail(username))
            {
                type = UMC.Security.Account.EMAIL_ACCOUNT_KEY;

            }
            else if (Data.Utility.IsPhone(username))
            {
                type = UMC.Security.Account.MOBILE_ACCOUNT_KEY;
            }
            else
            {
                this.Prompt("只支持手机号和邮箱找回密码");
            }
            var acct = Data.DataFactory.Instance().Account(username); //entity.Single();
            if (acct == null)
            {
                switch (type)
                {
                    case UMC.Security.Account.EMAIL_ACCOUNT_KEY:
                        this.Prompt("没有找到此邮箱绑定账户");
                        break;
                    default:
                        this.Prompt("没有找到此手机号绑定账户");
                        break;
                }
            }
            else if (acct.Type.Value != type)
            {
                this.Prompt("只支持手机号和邮箱找回密码");

            }
            var Code = UMC.Web.UIDialog.AsyncDialog(this.Context, "Code", g =>
             {
                 var ts = type == UMC.Security.Account.EMAIL_ACCOUNT_KEY ? "邮箱" : "手机";
                 var fd = new UMC.Web.UIFormDialog();
                 fd.AddTextValue().Put(ts, username);

                 fd.AddVerify("验证码", "Code", String.Format("{0}收到的验证码", ts))
                  .Command(request.Model, request.Command, new UMC.Web.WebMeta().Put("Username", username).Put("Code", "Reset"));
                 fd.Title = "验证" + ts; 
                 fd.Submit("验证", "Forget");

                 return fd;
             });
            if (String.Equals(Code, "Reset"))
            {
                ;
                if (this.Send(username))
                {
                    this.Prompt("验证码已经发送，请注意查收", false);
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("VerifyCode", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("text", "验证码已发送")), true);
                }
                else
                {
                    switch (type)
                    {
                        case UMC.Security.Account.EMAIL_ACCOUNT_KEY:
                            this.Prompt("没有找到此邮箱绑定账户");
                            break;
                        default:
                            this.Prompt("没有找到此手机号绑定账户");
                            break;
                    }
                }
            }
            var account = Account.Create(acct);
            var VerifyCode = account.Items[Account.KEY_VERIFY_FIELD] as string;

            if (String.Equals(VerifyCode, Code, StringComparison.CurrentCultureIgnoreCase))
            {
                WebMeta print = new UMC.Web.WebMeta();
                print["AccountType"] = acct.Type.ToString();
                print["VerifyCode"] = Code;
                print["user_id"] = acct.user_id.ToString();

                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Forget"), false);
                response.Redirect(request.Model, "Password", print, true);
            }
            else
            {
                this.Prompt("您输入的验证码错误");
            }


        }

    }
}