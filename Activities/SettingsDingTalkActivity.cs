using System;
using UMC.Security;
using UMC.Web;


namespace UMC.Activities
{
    /*
     * 后台登录，进入管理后台
     *
     * */

    [Mapping("Settings", "DingTalk", Auth = WebAuthType.User, Desc = "钉钉API")]
    class SettingsDingTalkActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Path = this.AsyncDialog("Path", r => this.DialogValue("none"));
            var Method = this.AsyncDialog("Method", r => this.DialogValue("POST"));
            var accessToken = UMC.Configuration.DingTalk.AccessToken();
            var sb = new System.Text.StringBuilder();
            sb.Append(Path);
            sb.Append("?access_token=");
            sb.Append(accessToken);

            var senders = request.SendValues ?? new WebMeta();
            senders.Remove("Path");
            senders.Remove("Method");
            switch (Method)
            {
                case "GET":
                    var d = senders.GetDictionary().GetEnumerator();
                    while (d.MoveNext())
                    {
                        sb.AppendFormat("&{0}={1}", d.Key, d.Value);
                    }
                    response.Redirect(UMC.Data.JSON.Expression(UMC.Configuration.DingTalk.Get(sb.ToString())));
                    break;
                default:
                    response.Redirect(UMC.Data.JSON.Expression(UMC.Configuration.DingTalk.Post(sb.ToString(), senders)));
                    break;
            }
        }
    }
}