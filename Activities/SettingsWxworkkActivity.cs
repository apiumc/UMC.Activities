using System;
using UMC.Security;
using UMC.Web;


namespace UMC.Activities
{
    

    [Mapping("Settings", "Wxwork", Auth = WebAuthType.User, Desc = "企业微信API")]
    class SettingsWxworkkActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Path = this.AsyncDialog("Path", r => this.DialogValue("none"));
            var Method = this.AsyncDialog("Method", r => this.DialogValue("POST"));
            var accessToken = UMC.Configuration.Corp.AccessToken();
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
                    response.Redirect(UMC.Data.JSON.Expression(UMC.Configuration.Corp.Get(sb.ToString())));
                    break;
                default:
                    response.Redirect(UMC.Data.JSON.Expression(UMC.Configuration.Corp.Post(sb.ToString(), senders)));
                    break;
            }
        }
    }
}