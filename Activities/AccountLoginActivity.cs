using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Data;
using UMC.Web;
using UMC.Net;
using System.Security.Cryptography;
using UMC.Configuration;

namespace UMC.Activities
{
    class AccountLoginActivity : WebActivity
    {
        void Dingtalk(String code, string corpId, int timeout)
        {

            var accessToken = UMC.Configuration.DingTalk.AccessToken(corpId);
            var url = String.Format("user/getuserinfo?access_token={1}&code={0}", code, accessToken);

            var data = UMC.Data.JSON.Deserialize(DingTalk.Get(url)) as Hashtable;

            if (data.ContainsKey("userid"))
            {
                var userId = data["userid"] as string;

                DingtalkUser(userId, accessToken, timeout);
            }
            else
            {
                this.Prompt($"授权码不正确:{data["errmsg"]}");
            }
        }
        public static string ComputeSignature(string secret, string canonicalString)
        {
            return Convert.ToBase64String(Sign(Encoding.UTF8.GetBytes(canonicalString), Encoding.UTF8.GetBytes(secret)));
        }
        private static byte[] Sign(byte[] key, byte[] data)
        {
            HMACSHA256 hmacsha = new HMACSHA256(data);
            return hmacsha.ComputeHash(key);
        }
        void DingtalkApp(string code)
        {
            var appconfig = UMC.Configuration.DingTalk.AppConfig();
            var t = UMC.Data.Reflection.TimeSpanMilli(DateTime.Now).ToString();
            var apspt = appconfig["appsecret"];
            var appid = appconfig["appid"];


            var url = String.Format("sns/getuserinfo_bycode?signature={0}&timestamp={1}&accessKey={2}", Uri.EscapeDataString(ComputeSignature(apspt, t)), t, appid);

            var hash = JSON.Deserialize(DingTalk.Post(url, new WebMeta().Put("tmp_auth_code", code))) as Hashtable;
            var user = hash["user_info"] as Hashtable;
            if (user.ContainsKey("unionid") == false)
            {
                this.Prompt("接口错误，请联系管理员");
            }
            var forid = appconfig["forid"];
            if (String.IsNullOrEmpty(forid) == false)
            {

                var s = UMC.Configuration.DingTalk.AccessToken(forid);
                var sUrl = String.Format("user/getUseridByUnionid?access_token={0}&unionid={1}", s, user["unionid"]);// response.UserInfo.Unionid);
                var uResult = DingTalk.Get(sUrl);
                var us = JSON.Deserialize(uResult) as Hashtable;



                if (us.ContainsKey("userid"))
                {
                    DingtalkUser(us["userid"] as String, s, 0);

                }
                else
                {
                    this.Prompt(String.Format("您不在{0}组织架构中", appconfig["title"]));
                }
            }
            else
            {
                Login(user["unionid"] as String, user["nick"] as String, 0);

            }

        }
        void Login(String username, String alias, int timeout)
        {
            this.Login(username, alias, timeout, null);
        }
        void Login(String username, String alias, int timeout, String hearUrl)
        {
            var userManager = UMC.Security.Membership.Instance();
            var iden = userManager.Identity(username);
            if (iden == null)
            {
                iden = UMC.Security.Identity.Create(UMC.Data.Utility.Guid(username, true).Value, username, alias, UMC.Security.Membership.UserRole);
            }
            if (String.IsNullOrEmpty(hearUrl) == false)
            {
                UMC.Data.WebResource.Instance().Transfer(new Uri(hearUrl), iden.Id.Value, 1);


            }
            var callback = this.Context.Token.Data[("oauth_callback")] as string;
            var request = this.Context.Request;
            if (timeout > 0)
            {
                this.Context.Token.Login(iden
                    , timeout, request.IsApp ? "App" : "Desktop", true, request.UserHostAddress);
            }
            else
            {
                this.Context.Token.Login(iden,  request.IsApp ? "App" : "Desktop", true, request.UserHostAddress);
            }
            if (String.IsNullOrEmpty(this.transfer) == false)
            {
                var sesion = UMC.Data.DataFactory.Instance().Session(this.Context.Token.Id.ToString());

                if (sesion != null)
                {
                    sesion.SessionKey = this.transfer;

                    UMC.Data.DataFactory.Instance().Post(sesion);
                    WebResource.Instance().Push(UMC.Data.Utility.Guid(this.transfer, true).Value, new WebMeta().Put("msg", "OK"));
                }

            }

            if (String.IsNullOrEmpty(callback) == false)
            {
                this.Context.Send("User", new WebMeta().Put("Url", callback), true);
            }
            this.Context.Send("User", true);
        }
        string transfer;
        void DingtalkUser(String userId, String accessToken, int timeout)
        {
            var data2 = UMC.Data.JSON.Deserialize(DingTalk.Get(String.Format("user/get?access_token={1}&userid={0}", userId, accessToken))) as Hashtable;


            var avatar = data2["avatar"] as string;

            var jobnumber = data2["jobnumber"] as string;

            var name = data2["name"] as string;
            if (String.IsNullOrEmpty(jobnumber))
            {
                jobnumber = data2["mobile"] as string ?? userId;

            }
            if (String.IsNullOrEmpty(avatar) == false)
            {
                UMC.Data.WebResource.Instance().Transfer(new Uri(avatar), UMC.Data.Utility.Guid(jobnumber, true).Value, 1);
            }

            Login(jobnumber, name, timeout);
        }
        void WxCorp(String code, int timeout)
        {
            Provider wxProvider = null;
            var login = (UMC.Data.DataFactory.Instance().Configuration("account") ?? new ProviderConfiguration()).Providers.GetEnumerator();
            while (login.MoveNext())
            {
                Provider provider = (Provider)login.Value;

                if (String.Equals(provider.Type, "wxwork"))
                {
                    wxProvider = provider;
                    break;
                }
            }
            if (wxProvider == null)
            {
                this.Prompt("未配置企业微信");
            }
            var accessToken = UMC.Configuration.Corp.AccessToken(wxProvider["appid"]);
            var url = String.Format("cgi-bin/user/getuserinfo?access_token={1}&code={0}", code, accessToken);

            var data = UMC.Data.JSON.Deserialize(Corp.Get(url)) as Hashtable;

            if (data.ContainsKey("UserId"))
            {
                var userId = data["UserId"] as string;

                var url2 = String.Format("cgi-bin/user/get?access_token={1}&userid={0}", userId, accessToken);
                var data2 = UMC.Data.JSON.Deserialize(Corp.Get(url2)) as Hashtable;


                var jobnumber = data2["mobile"] as string;
                if (String.IsNullOrEmpty(jobnumber) && UMC.Data.Utility.IsPhone(userId))
                {
                    jobnumber = userId;
                }
                if (String.IsNullOrEmpty(jobnumber) == false)
                {
                    var forid = wxProvider["forid"];
                    if (String.IsNullOrEmpty(forid) == false)
                    {
                        var accessToken2 = UMC.Configuration.DingTalk.AccessToken(forid);

                        var data3 = UMC.Data.JSON.Deserialize(DingTalk.Get(String.Format("user/get_by_mobile?access_token={1}&mobile={0}", jobnumber, accessToken2))
                         ) as Hashtable;
                        if (data3.ContainsKey("userid"))
                        {
                            userId = data3["userid"] as string;
                            DingtalkUser(userId, accessToken2, timeout);
                            return;
                        }
                    }
                }
                var avatar = data2["avatar"] as string;


                var name = data2["name"] as string;
                if (String.IsNullOrEmpty(jobnumber))
                {
                    jobnumber = data2["mobile"] as string ?? userId;

                }
                if (String.IsNullOrEmpty(avatar) == false)
                {
                    UMC.Data.WebResource.Instance().Transfer(new Uri(avatar), UMC.Data.Utility.Guid(jobnumber, true).Value, 1);
                }
                Login(jobnumber, name, timeout);
            }
            else
            {
                this.Prompt("微信企业号登录不正确");

            }



        }
        void SendMobileCode(string mobile)
        {

            var user = this.Context.Token.Identity(); //UMC.Security.Identity.Current;



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
                    this.Prompt("您已经超过了5次，请三小时后再试");

                }
            }
            session.Commit(hask, user, this.Context.Request.UserHostAddress);


            hask["DateTime"] = DateTime.Now;

            Net.Message.Instance().Send("Login", hask, mobile);


        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var type = this.AsyncDialog("type", t => this.DialogValue("auto"));

            var login = (UMC.Data.DataFactory.Instance().Configuration("account") ?? new ProviderConfiguration())["login"] ?? Provider.Create("name", "name");
            var timeout = UMC.Data.Utility.IntParse(login.Attributes["timeout"], 3600);
            var user = Web.UIFormDialog.AsyncDialog(this.Context, "Login", d =>
           {
               if (request.SendValues != null && request.SendValues.Count > 0)
               {
                   return this.DialogValue(request.SendValues);
               }
               if (request.Url.Query.Contains("_v=Sub"))
               {
                   this.Context.Send("Login", true);
               }
               switch (type)
               {
                   case "wx":
                       this.Context.Send(new UMC.Web.WebMeta().Put("type", "login.weixin"), true);
                       break;
                   case "qq":
                       this.Context.Send(new UMC.Web.WebMeta().Put("type", "login.qq"), true);
                       break;
               }
               if (request.IsApp)
               {
                   var loginData = new WebMeta();
                   var appid = login.Attributes[type];
                   if (String.IsNullOrEmpty(appid) == false)
                   {
                       loginData.Put("appid", appid);
                   }
                   switch (type)
                   {
                       case "weixin":
                       case "qq":
                       case "dingtalk":
                           this.Context.Send("login." + type, loginData, true);
                           break;
                       default:
                           var appLogin = login["default"];
                           if (String.IsNullOrEmpty(appLogin) == false)
                           {
                               appid = login.Attributes[appLogin];
                               if (String.IsNullOrEmpty(appid) == false)
                               {
                                   loginData.Put("appid", appid);
                               }
                               this.Context.Send("login." + appLogin, loginData, true);
                           }
                           break;
                   }


               }

               var dialog = new Web.UIFormDialog();
               dialog.Title = "登录";
               switch (type)
               {
                   default:
                   case "User":
                       this.Context.Send("LoginChange", false);
                       {
                           dialog.AddText("用户名", "Username", String.Empty).Put("placeholder", "用户名/手机/邮箱");

                           dialog.AddPassword("用户密码", "Password", String.Empty);

                           dialog.Submit("登录", request, "User", "LoginChange");
                           var uidesc = new UMC.Web.UI.UIDesc(new WebMeta().Put("eula", "用户协议").Put("private", "隐私政策"));
                           uidesc.Desc("登录即同意“{eula}”和“{private}”");
                           uidesc.Style.AlignCenter();
                           uidesc.Style.Color(0x888).Size(14).Height(34);
                           uidesc.Style.Name("eula").Color(0x3194d0).Click(new UIClick("365lu/provision/eula").Send("Subject", "UIData"));
                           uidesc.Style.Name("private").Color(0x3194d0).Click(new UIClick("365lu/provision/private").Send("Subject", "UIData"));
                           dialog.Add(uidesc);
                           dialog.AddUIIcon("\uf2c1", "免密登录").Command(request.Model, request.Command, "Mobile");
                           dialog.AddUIIcon("\uf1c6", "忘记密码").Put("Model", request.Model).Put("Command", "Forget");
                           dialog.AddUIIcon("\uf234", "注册新用户").Put("Model", request.Model).Put("Command", "Register");

                       }
                       break;
                   case "Mobile":
                       this.Context.Send("LoginChange", false);
                       {
                           dialog.AddText("手机号码", "Username", String.Empty).Put("placeholder", "注册的手机号码");

                           dialog.AddVerify("验证码", "VerifyCode", "您收到的验证码").Put("For", "Username").Put("To", "Mobile")
                           .Put("Command", request.Command).Put("Model", request.Model);
                           dialog.Submit("登录", request, "User", "LoginChange");

                           var uidesc = new UMC.Web.UI.UIDesc(new WebMeta().Put("eula", "用户协议").Put("private", "隐私政策"));
                           uidesc.Desc("登录即同意“{eula}”和“{private}”");
                           uidesc.Style.AlignCenter();
                           uidesc.Style.Color(0x888).Size(14).Height(34);
                           uidesc.Style.Name("eula").Color(0x3194d0).Click(new UIClick("365lu/provision/eula").Send("Subject", "UIData"));
                           uidesc.Style.Name("private").Color(0x3194d0).Click(new UIClick("365lu/provision/private").Send("Subject", "UIData"));
                           dialog.Add(uidesc);
                           dialog.AddUIIcon("\uf13e", "密码登录").Command(request.Model, request.Command, "User");
                           dialog.AddUIIcon("\uf234", "注册新用户").Command(request.Model, "Register");

                       }
                       break;
               }

               return dialog;


           });

            if (user.ContainsKey("Mobile"))
            {
                var mobile = user["Mobile"];

                var account = Data.DataFactory.Instance().Account(mobile, UMC.Security.Account.MOBILE_ACCOUNT_KEY);
                if (account == null)
                {
                    this.Prompt("不存在此账户");
                }


                this.SendMobileCode(mobile);
                this.Prompt("验证码已发送", false);
                this.Context.Send(new UMC.Web.WebMeta().UIEvent("VerifyCode", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("text", "验证码已发送")), true);
            }

            var username = user["Username"];

            var userManager = UMC.Security.Membership.Instance();
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

                var eData = Data.DataFactory.Instance().Account(username, UMC.Security.Account.MOBILE_ACCOUNT_KEY);
                if (eData == null)
                {

                    this.Prompt("无此号码关联的账户，请注册");
                }
                else
                {
                    var iden = userManager.Identity(eData.user_id.Value);

                    this.Context.Token.Login(iden,   request.IsApp ? 0 : timeout, request.IsApp ? "App" : "Desktop", true, request.UserHostAddress);
                    this.Context.Send("User", true);
                }
            }
            else if (user.ContainsKey("code"))
            {
                var code = user["code"];
                var appid = user["appid"];
                this.transfer = user["transfer"];
                //var url = login.Attributes["login"];
                //if (String.IsNullOrEmpty(url))
                //{
                switch (type)
                {
                    case "dingtalk":
                        Dingtalk(code, appid, UMC.Data.Utility.IntParse(user["timeout"], timeout));
                        break;
                    case "dingtalk.app":
                        DingtalkApp(code);
                        break;
                    case "wxwork":
                        WxCorp(code, UMC.Data.Utility.IntParse(user["timeout"], timeout));
                        break;
                }
                this.Prompt("未配置第三方系统对接");
                ////}
                //var sb = new StringBuilder();
                //var em = user.GetDictionary().GetEnumerator();
                //sb.Append(url);
                //var isOne = url.IndexOf('?') == -1;
                //while (em.MoveNext())
                //{
                //    if (isOne)
                //    {
                //        sb.Append("?");
                //        isOne = false;
                //    }
                //    else
                //    {
                //        sb.Append("&");
                //    }
                //    sb.Append(Uri.UnescapeDataString(em.Key.ToString()));
                //    sb.Append("=");
                //    sb.Append(Uri.UnescapeDataString(em.Value.ToString()));


                //}
                //sb.Append("&type=");
                //sb.Append(Uri.UnescapeDataString(type));
                //if (request.IsApp)
                //{
                //    sb.Append(".app");
                //}
                //var http = new Uri(sb.ToString()).WebRequest();
                //http.UserAgent = request.UserAgent;

                //var ress = UMC.Data.JSON.Deserialize<Hashtable>(http.Get().ReadAsString());
                //if (ress.ContainsKey("msg"))
                //{
                //    this.Prompt(ress["msg"] as string);
                //}
                //else if (ress.ContainsKey("Username"))
                //{
                //    var uName = ress["Username"] as string;
                //    var Alias = ress["Alias"] as string;

                //    if (ress.ContainsKey("Src"))
                //    {
                //        Login(uName, Alias, UMC.Data.Utility.IntParse(user["timeout"], timeout), ress["Src"] as string);
                //    }
                //    else
                //    {
                //        Login(uName, Alias, UMC.Data.Utility.IntParse(user["timeout"], timeout));

                //    }
                //}

            }
            else
            {
                var passwork = user["Password"];

                var maxTimes = 5;
                UMC.Security.Identity identity = null;
                if (UMC.Data.Utility.IsPhone(username))
                {
                    identity = userManager.Identity(username, Security.Account.MOBILE_ACCOUNT_KEY) ?? userManager.Identity(username);
                }
                else if (username.IndexOf('@') > -1)
                {
                    identity = userManager.Identity(username, Security.Account.EMAIL_ACCOUNT_KEY) ?? userManager.Identity(username);
                }
                else
                {
                    identity = userManager.Identity(username);
                }
                if (identity == null)
                {
                    this.Prompt("用户不存在，请确认用户名");
                }
                var times = userManager.Password(identity.Name, passwork, maxTimes);
                switch (times)
                {
                    case 0:
                        var iden = userManager.Identity(username);
                        this.Context.Token.Login(iden,   request.IsApp ? 0 : timeout, request.IsApp ? "App" : "Desktop", true, request.UserHostAddress);
                        this.Context.Send("User", true);
                        break;
                    case -2:
                        this.Prompt("您的用户已经锁定，请过后登录");
                        break;
                    case -1:
                        this.Prompt("您的用户不存在，请确定用户名");
                        break;
                    default:
                        this.Prompt(String.Format("您的用户和密码不正确，您还有{0}次机会", maxTimes - times));

                        break;
                }
            }
        }

    }
}