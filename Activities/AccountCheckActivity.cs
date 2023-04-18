


using System;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class AccountCheckActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = this.Context.Token.Identity();
            var checkValue = this.AsyncDialog("Key", "Check");
            switch (checkValue)
            {
                case "Mqtt":
                    response.Redirect(UMC.Data.WebResource.Instance().Push(this.Context.Token.Device.Value));
                    break;
                case "Session":
                    var seesionKey = UMC.Data.Utility.Guid(this.Context.Token.Device.Value);
                    var seesion = UMC.Data.DataFactory.Instance().Session(seesionKey);

                    if (seesion != null)
                    {
                        var login = Reflection.Configuration("account")["login"] ?? Provider.Create("name", "name");
                        var timeout = UMC.Data.Utility.IntParse(login.Attributes["timeout"], 3600);


                        var Value = UMC.Data.JSON.Deserialize<UMC.Data.AccessToken>(seesion.Content);
                        user = Value.Identity();
                        UMC.Data.DataFactory.Instance().Delete(seesion);
                        var strQuery = request.UrlReferrer?.Query;
                        if (String.IsNullOrEmpty(strQuery) == false)
                        {
                            var query = System.Web.HttpUtility.ParseQueryString(strQuery);
                            var transfer = query["transfer"];
                            if (String.IsNullOrEmpty(transfer) == false)
                            {
                                seesion.SessionKey = transfer;
                                UMC.Data.DataFactory.Instance().Put(seesion);
                            }
                        }
                        this.Context.Token.Login(user, timeout).Commit(request.IsApp ? "App" : "Desktop", true, request.UserHostAddress, this.Context.Server);
                        this.Context.Send("User", new WebMeta("Alias", user.Alias).Put("Src", Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", 5)), true);

                    }
                    else
                    {

                        if (request.IsCashier)
                        {
                            this.Context.Send("User", new WebMeta("Alias", user.Alias).Put("Src", Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", 5)), true);
                        }
                        else
                        {
                            response.Redirect(new WebMeta().Put("Device", UMC.Data.Utility.Guid(this.Context.Token.Device.Value)));
                        }
                    }
                    return;
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
                case "Check":
                case "Event":
                    break;
                default:
                case "Info":
                    var info = new System.Collections.Hashtable();
                    if (user.IsAuthenticated)
                    {
                        info["Name"] = user.Name;
                        info["Alias"] = user.Alias;
                        info["Src"] = UMC.Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", (object)4);
                    }
                    if (String.Equals(checkValue, "Info") == false)
                    {
                        this.Context.Token.Put("SPM", checkValue).Commit(request.UserHostAddress, this.Context.Server);
                    }
                    info["IsCashier"] = request.IsCashier;
                    info["IsMaster"] = request.IsMaster;
                    info["TimeSpan"] = UMC.Data.Utility.TimeSpan();
                    info["Device"] = UMC.Data.Utility.Guid(this.Context.Token.Device.Value);

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

            }

            if (user.IsAuthenticated == false)
            {
                if (checkValue == "Event")
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