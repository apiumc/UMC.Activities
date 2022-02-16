using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Data;
using UMC.Security;
using UMC.Web;

namespace UMC.Activities
{
    class AccountMobileActivity : WebActivity
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


            UMC.Data.Reflection.PropertyToDictionary(Data.DataFactory.Instance().User(user.Id.Value), hask);
            //UMC.Data.Reflection.PropertyToDictionary(UMC.Data.Database.Instance().ObjectEntity<UMC.Data.Entities.User>()
            //    .Where.And().Equal(new UMC.Data.Entities.User { Id = user.Id.Value }).Entities.Single(), hask);

            hask["DateTime"] = DateTime.Now;

            Net.Message.Instance().Send("Verify", hask, mobile);

        }


        void Remove()
        {
            var user = UMC.Security.Identity.Current;
            var act = Account.Create(user.Id.Value);

            var a = act[Account.MOBILE_ACCOUNT_KEY];
            var code = Web.UIDialog.AsyncDialog("Remove", d =>
                 {

                     var fm = new Web.UIFormDialog() { Title = "解除验证" };
                     fm.AddTextValue().Put("手机号码", a.Name);
                     fm.AddVerify("验证码", "Code", "您收到的验证码")
                    .Command("Account", "Mobile", new UMC.Web.WebMeta().Put("Mobile", a.Name).Put("Code", "Send"))
                    .Put("Start", "YES");

                     fm.Submit("确认验证码", this.Context.Request, "Mobile");
                     return fm;
                 });
            var session = new UMC.Data.Session<Hashtable>(a.Name);
            if (session.Value != null)
            {
                if (String.Equals(session.Value["Code"] as string, code))
                {
                    Account.Post(a.Name, a.user_id, Security.UserFlags.UnVerification, Account.MOBILE_ACCOUNT_KEY);
                    this.Prompt("手机解除绑定成功", false);
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "Mobile"), true);
                }
            }
            this.Prompt("您输入的验证码错误");
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var user = UMC.Security.Identity.Current;
            var act = Account.Create(user.Id.Value);


            var value = Web.UIDialog.AsyncDialog("Mobile", d =>
            {
                var acc = act[UMC.Security.Account.MOBILE_ACCOUNT_KEY];
                if (acc != null && (acc.Flags & UserFlags.UnVerification) != UserFlags.UnVerification)
                {
                    return new Web.UIConfirmDialog("您确认解除与手机的绑定吗") { Title = "解除确认", DefaultValue = "Change" };
                }
                var fm = new Web.UIFormDialog() { Title = "手机绑定" };
                fm.AddText("手机号码", "Mobile", acc != null ? acc.Name : "");

                fm.Submit("下一步", request, "Mobile");
                return fm;

            });
            switch (value)
            {
                case "Change":

                    Remove();
                    return;
            }
            if (Data.Utility.IsPhone(value) == false)
            {
                this.Prompt("手机号码格式不正确");
            }


            var mobile = Data.DataFactory.Instance().Account(value, UMC.Security.Account.MOBILE_ACCOUNT_KEY);

            if (mobile != null && mobile.user_id.Value != user.Id.Value)
            {
                this.Prompt("此手机号码已被其他账号绑定");

            }


            var Code = UMC.Web.UIDialog.AsyncDialog("Code", g =>
            {
                var fm = new Web.UIFormDialog() { Title = "手机号码验证" };
                fm.AddTextValue().Put("手机号码", value);
                fm.AddVerify("验证码", "Code", "您收到的验证码")
                .Command("Account", "Mobile", new UMC.Web.WebMeta().Put("Mobile", value).Put("Code", "Send"))
                    .Put("Start", "YES");

                fm.Submit("确认验证码", request, "Mobile");
                return fm;
            });
            if (Code == "Send")
            {
                this.SendMobileCode(value);
                this.Prompt("验证码已发送", false);
                this.Context.Send(new UMC.Web.WebMeta().UIEvent("VerifyCode", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta("Time", "100")), true);

            }
            var session = new UMC.Data.Session<Hashtable>(value);
            if (session.Value != null)
            {
                var code = session.Value["Code"] as string;
                if (String.Equals(code, Code))
                {

                    Account.Post(value, user.Id.Value, Security.UserFlags.Normal, Account.MOBILE_ACCOUNT_KEY);
                    this.Prompt("手机号码绑定成功", false);
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "Mobile"), true);
                }
            }


            this.Prompt("您输入的验证码错误");


        }

    }
}