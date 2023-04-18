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
    class AccountEmailActivity : WebActivity
    {

        void SendEmail(string email)
        {
            var user = this.Context.Token.Identity();

            var uiattr = UMC.Data.Reflection.GetDataProvider("account", "Email");


            var hask = new Hashtable();

            hask["Code"] = Utility.NumberCode(Guid.NewGuid().GetHashCode(), 6);
            hask["DateTime"] = DateTime.Now;

            var session = new UMC.Data.Session<Hashtable>(email);
            if (session.ModifiedTime.AddMinutes(15) > DateTime.Now)
            {
                hask = session.Value;
            }
            else
            {
                session.Commit(hask, user, this.Context.Request.UserHostAddress);
            }

            UMC.Data.Utility.AppendDictionary(Data.DataFactory.Instance().User(user.Id.Value), hask);


            var mail = new System.Net.Mail.MailMessage();
            mail.To.Add(email);
            mail.Subject = Utility.Format(uiattr["Subject"], hask);
            mail.Body = Utility.Format(uiattr["Body"], hask);
            mail.IsBodyHtml = true;
            Net.Message.Instance().Send(mail);

        }

        void Remove()
        {
            var user = this.Context.Token.Identity();
            var act = Account.Create(user.Id.Value);

            var a = act[Account.EMAIL_ACCOUNT_KEY];
            var code = this.AsyncDialog("Remove", d =>
            {

                var fm = new Web.UIFormDialog() { Title = "解除验证" };
                fm.AddTextValue().Put("邮箱", a.Name);
                fm.AddVerify("验证码", "Code", "您邮箱收到的验证码")
                    .Put("Command", "Email").Put("Model", "Account").Put("SendValue", new UMC.Web.WebMeta().Put("Email", a.Name).Put("Code", "Send")).Put("Start", "YES");

                fm.Submit("确认验证码", $"{this.Context.Request.Model}.{this.Context.Request.Command}");
                return fm;
            });
            var session = new UMC.Data.Session<Hashtable>(a.Name);
            if (session.Value != null)
            {
                if (String.Equals(session.Value["Code"] as string, code))
                {

                    Account.Post(a.Name, a.user_id, Security.UserFlags.UnVerification, Account.EMAIL_ACCOUNT_KEY);
                    this.Prompt("邮箱解除绑定成功", false);
                    this.Context.Send($"{this.Context.Request.Model}.{this.Context.Request.Command}", true);
                }
            }
            this.Prompt("您输入的验证码错误");
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var user = this.Context.Token.Identity();
            var act = Account.Create(user.Id.Value);


            var value = this.AsyncDialog("Email", d =>
            {
                var acc = act[UMC.Security.Account.EMAIL_ACCOUNT_KEY];
                if (acc != null && (acc.Flags & UserFlags.UnVerification) != UserFlags.UnVerification)
                {
                    return new Web.UIConfirmDialog("您确认解除与邮箱的绑定吗") { Title = "解除确认", DefaultValue = "Change" };
                }


                var t = new Web.UITextDialog() { Title = "邮箱绑定", DefaultValue = acc != null ? acc.Name : "" };
                t.SubmitText = "下一步";
                return t;

            });
            switch (value)
            {
                case "Change":

                    Remove();
                    return;
            }

            if (Data.Utility.IsEmail(value) == false)
            {
                this.Prompt("邮箱格式不正确");
            }


            var email = Data.DataFactory.Instance().Account(value);
            if (email != null && email.user_id.Value != user.Id.Value)
            {
                this.Prompt("此邮箱已存在绑定");

            }


            var Code = UMC.Web.UIDialog.AsyncDialog(this.Context, "Code", g =>
           {
               var fm = new Web.UIFormDialog() { Title = "验证码" };
               fm.AddTextValue().Put("邮箱", value);
               fm.AddVerify("验证码", "Code", "您邮箱收到的验证码")
                   .Put("Command", "Email").Put("Model", "Account").Put("SendValue", new UMC.Web.WebMeta().Put("Email", value).Put("Code", "Send")).Put("Start", "YES");

               fm.Submit("确认验证码", $"{this.Context.Request.Model}.{this.Context.Request.Command}");
               return fm;
           });
            if (Code == "Send")
            {
                this.SendEmail(value);
                this.Prompt("验证码已发送");
            }
            var session = new UMC.Data.Session<Hashtable>(value);
            if (session.Value != null)
            {
                var code = session.Value["Code"] as string;
                if (String.Equals(code, Code))
                {

                    Account.Post(value, user.Id.Value, Security.UserFlags.Normal, Account.EMAIL_ACCOUNT_KEY);
                    this.Prompt("邮箱绑定成功", false);
                    this.Context.Send($"{this.Context.Request.Model}.{this.Context.Request.Command}", true);
                }
            }


            this.Prompt("您输入的验证码错误");


        }

    }
}