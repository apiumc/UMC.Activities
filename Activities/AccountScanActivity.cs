using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using UMC.Web;
using UMC.Data;
using UMC.Security;

namespace UMC.Activities
{
    class AccountScanActivity : WebActivity
    {
        WebMeta Config()
        {
            var request = this.Context.Request;


            var account = UMC.Data.Reflection.Configuration("account");

            var hash = new HashSet<String>();
            for (var i = 0; i < account.Count; i++)
            {
                var p = account[i];
                switch (p.Type)
                {
                    case "dingtalk":
                        hash.Add("钉钉");
                        break;
                    case "wxwork":
                        hash.Add("企业微信");
                        break;
                }
            }
            var webConfig = new WebMeta();
            if (hash.Count > 0)
            {
                var scan = UMC.Data.WebResource.Instance().Push(this.Context.Token.Device.Value);
                scan.Put("title", String.Join("、", hash));
                scan.Put("url", new Uri(request.UrlReferrer ?? request.Url, $"/UMC/Account/Scan/{UMC.Data.Utility.Guid(this.Context.Token.Device.Value)}").AbsoluteUri);

                webConfig.Put("scan", scan);
            }
            webConfig.Put("bgsrc", UMC.Data.WebResource.Instance().Provider["bgsrc"]);
            webConfig.Put("form", true);
            return webConfig;
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var device = this.AsyncDialog("state", "none");
            if (String.Equals(device, "none"))
            {
                response.Redirect(Config());

            }
            var code = this.AsyncDialog("code", "none");
            var appid = this.AsyncDialog("appid", "none");
            var ua = request.UserAgent.ToUpper();
            if (String.Equals(code, "none"))
            {
                if (ua.Contains("WXWORK") || ua.Contains("MICROMESSENGER"))
                {
                    var account = UMC.Data.Reflection.Configuration("account");
                    var appids = new List<String>();
                    for (var i = 0; i < account.Count; i++)
                    {
                        var p = account[i];
                        if (String.Equals(p.Type, "wxwork"))
                        {

                            appids.Add(p.Name);
                        }
                    }
                    if (appids.Count > 0)
                    {
                        var redirect_uri = Uri.EscapeUriString(new Uri(request.UrlReferrer ?? request.Url, $"/UMC/wxwork.html?appid={appids[0]}").AbsoluteUri);

                        WebResource.Instance().Push(UMC.Data.Utility.Guid(device, true).Value
                            , new WebMeta().Put("msg", "已经扫码成功"));
                        var wxP = account[appids[0]];
                        var agentid = wxP["agentid"];
                        var urlStr = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appids[0]}&response_type=code&scope=snsapi_base&state={device}&redirect_uri={redirect_uri}#wechat_redirect";

                        if (String.IsNullOrEmpty(agentid) == false)
                        {
                            urlStr = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appids[0]}&response_type=code&agentid={agentid}&scope=snsapi_privateinfo&state={device}&redirect_uri={redirect_uri}#wechat_redirect";

                        }
                        if (request.Url.AbsolutePath.Contains(device))
                        {
                            response.Redirect(new Uri(urlStr));

                        }
                        else
                        {
                            this.Context.Send("OpenUrl", new WebMeta().Put("value", urlStr), true);

                        }
                    }
                    else
                    {
                        this.Prompt("提示", "未配置企业微信登录");
                    }
                }
                else if (ua.Contains("DINGTALK"))
                {
                    var account = UMC.Data.Reflection.Configuration("account");
                    var appids = new List<String>();
                    for (var i = 0; i < account.Count; i++)
                    {
                        var p = account[i];
                        if (String.Equals(p.Type, "dingtalk"))
                        {

                            appids.Add(p.Name);
                        }
                    }
                    if (appids.Count > 0)
                    {

                        WebResource.Instance().Push(UMC.Data.Utility.Guid(device, true).Value
                            , new WebMeta().Put("msg", "已经扫码成功"));

                        var url = $"/UMC/dingtalk.html?appid={String.Join(",", appids)}&state={device}";
                        if (request.Url.AbsolutePath.Contains(device))
                        {
                            response.Redirect(new Uri(request.UrlReferrer ?? request.Url, url));

                        }
                        else
                        {
                            this.Context.Send("OpenUrl", new WebMeta().Put("value", new Uri(request.UrlReferrer ?? request.Url, url).AbsoluteUri), true);

                        }
                    }
                    else
                    {
                        this.Prompt("提示", "未配置钉钉登录");
                    }
                }
                else
                {
                    this.Prompt("提示", "不支持此类型APP扫码登录");
                }

            }
            else if (ua.Contains("WXWORK") || ua.Contains("MICROMESSENGER"))
            {
                response.Redirect("Account", "Login", new WebMeta("appid", appid, "code", code, "transfer", device), true);
            }
            else
            {
                UMC.Web.UIDialog.AsyncDialog(this.Context, "Info", gg =>
                {
                    var style = new UIStyle();
                    style.Name("icon").Color(0x09bb07).Size(84).Font("wdk");
                    style.Name("title").Color(0x333).Size(20);
                    style.BgColor(0xfafcff).Height(200).AlignCenter();

                    var fom = new Web.UIFormDialog() { Title = "扫码登录" };
                    var strfs = "其他";
                    if (ua.Contains("WXWORK") || ua.Contains("MICROMESSENGER"))
                    {
                        strfs = "企业微信";
                    }
                    else if (ua.Contains("DINGTALK"))
                    {

                        strfs = "钉钉";
                    }


                    fom.AddTextValue().Put("所属体系", strfs);
                    var desc = new UMC.Web.WebMeta().Put("title", "扫一扫登录").Put("icon", "\uea04");

                    fom.Config.Put("Header", new UIHeader().Desc(desc, "{icon}\n{title}", style));

                    fom.AddPrompt($"确认后将采用{strfs}身份信息登录应用");
                    fom.Config.Put("Action", new UIClick("appid", appid, "code", code, "transfer", device).Send("Account", "Login"));
                    fom.Submit("确认登录");


                    return fom;
                });
            }

        }
    }
}