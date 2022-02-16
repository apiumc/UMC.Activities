


using System;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class AccountCheckActivity : WebActivity
    {



        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;
            switch (request.SendValue)
            {
                case "Mqtt":
                    response.Redirect(UMC.Data.WebResource.Instance().Push(UMC.Security.AccessToken.Token.Value));
                    break;
                case "Session":
                    var seesionKey = UMC.Data.Utility.Guid(UMC.Security.AccessToken.Token.Value);
                    UMC.Data.HotCache.Remove(new UMC.Data.Entities.Session { SessionKey = seesionKey });

                    var seesion = UMC.Data.DataFactory.Instance().Session(seesionKey);

                    if (seesion != null)
                    {
                        var login = (UMC.Data.DataFactory.Instance().Configuration("account") ?? new ProviderConfiguration())["login"] ?? Provider.Create("name", "name");
                        var timeout = UMC.Data.Utility.IntParse(login.Attributes["timeout"], 3600);


                        var Value = UMC.Data.JSON.Deserialize<UMC.Security.AccessToken>(seesion.Content);
                        user = Value.Identity();
                        UMC.Data.DataFactory.Instance().Delete(seesion);
                        UMC.Security.AccessToken.Login(user, UMC.Security.AccessToken.Token.Value, timeout, "Desktop", true);
                        this.Context.Send("User", true);
                    }
                    else
                    {

                        if (request.IsCashier)
                        {
                            this.Context.Send("User", true);

                        }
                        else
                        {
                            response.Redirect(new WebMeta().Put("Device", UMC.Data.Utility.Guid(UMC.Security.AccessToken.Token.Value)));
                        }
                    }
                    return;
                case "Info":
                    var info = new System.Collections.Hashtable();
                    if (user.IsAuthenticated)
                    {
                        info["Name"] = user.Name;
                        info["Alias"] = user.Alias;
                        info["Src"] = UMC.Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", (object)4);// user.Alias;
                    }
                    info["IsCashier"] = request.IsCashier;
                    info["IsMaster"] = request.IsMaster;
                    info["TimeSpan"] = UMC.Data.Utility.TimeSpan();
                    info["Device"] = UMC.Data.Utility.Guid(UMC.Security.AccessToken.Token.Value);// UMC.Data.Utility.Guid(UMC.Security.AccessToken.Token.Value);
                    if (String.IsNullOrEmpty(request.UserAgent) == false)
                    {
                        var ua = request.UserAgent.ToUpper();
                        if (ua.Contains("WXWORK"))
                        {
                            info["IsCorp"] = true;
                        }
                        else if (ua.Contains("MICROMESSENGER"))
                        {

                            info["IsWeiXin"] = true;
                        }
                        else if (ua.Contains("DINGTALK"))
                        {

                            info["IsDingTalk"] = true;
                        }
                    }
                    response.Redirect(info);
                    break;
                case "Login":
                    if (user.IsAuthenticated == false)
                    {
                        this.Context.Send("Login", true);


                    }
                    return;
                case "User":
                    if (request.IsCashier == false)
                    {
                        response.Redirect("Settings", "Login");


                    }
                    break;
                case "Client":
                    if (user.IsAuthenticated == false)
                    {
                        response.Redirect("Account", "Login");

                    }
                    else
                    {
                        response.Redirect("Account", "Self");

                    }
                    break;
                case "Cashier":
                    if (request.IsCashier == false)
                    {
                        response.Redirect("Settings", "Login");

                    }
                    else
                    {
                        response.Redirect("Account", "Self");

                    }
                    break;
                case "License":
                    if (request.IsMaster)
                    {

                        if (String.IsNullOrEmpty(WebResource.Instance().Provider["appId"]))
                        {
                            response.Redirect("System", "Register", new Web.UIConfirmDialog("当前应用未注册，请登记注册") { Title = "注册检验" });
                        }
                    }
                    return;

            }

            if (user.IsAuthenticated == false)
            {
                if (request.SendValue == "Event")
                {
                    this.Context.Send(new UMC.Web.WebMeta().Put("type", "Login"), true);
                }
                else
                {
                    response.Redirect("Account", "Login");
                }

            }
            this.Context.Send(new UMC.Web.WebMeta().UIEvent("Login", this.AsyncDialog("UI", "none"), new UMC.Web.WebMeta().Put("icon", "\uE91c", "format", "{icon}").Put("Alias", user.Alias).Put("click", new UIClick() { Command = "Self", Model = "Account" }).Put("style", new UIStyle().Name("icon", new UIStyle().Font("wdk")))), true);

        }
    }
}