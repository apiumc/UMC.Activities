using System;
using System.Collections.Generic;
using System.Collections;
using UMC.Data;
using UMC.Net;
using System.Text;

namespace UMC.Configuration
{

    public class WeiXin : UMC.Data.DataProvider
    {
        public WeiXin()
        {
        }
        public const int ACCOUNT_KEY = 10;

        public static int Color(String color)
        {
            switch (color)
            {
                default:
                case "Color010": return 0x63b359;
                case "Color020": return 0x2c9f67;
                case "Color030": return 0x509fc9;
                case "Color040": return 0x5885cf;
                case "Color050": return 0x9062c0;
                case "Color060": return 0xd09a45;
                case "Color070": return 0xe4b138;
                case "Color080": return 0xee903c;
                case "Color081": return 0xf08500;
                case "Color082": return 0xa9d92d;
                case "Color090": return 0xdd6549;
                case "Color100": return 0xcc463d;
                case "Color101": return 0xcf3e36;
                case "Color102": return 0x5E6671;

            }
        }

        class AccessToken
        {
            public string access_token
            {
                get;
                set;
            }
            public int expires_in
            {
                get;
                set;
            }
            public int ExpiresTime
            {
                get;
                set;
            }
        }
        static Dictionary<string, AccessToken> Tokens = new Dictionary<string, AccessToken>();

        public static string GetAccessToken(string appid, string appSecret)
        {

            if (Tokens.ContainsKey(appid))
            {
                var ac = Tokens[appid];
                if (ac.ExpiresTime > Data.Utility.TimeSpan(DateTime.Now))
                {
                    return ac.access_token;
                }
            }
            var text = Net.CURL.Create().Get(new Uri(String.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appid, appSecret)));

            var acc = Data.JSON.Deserialize<AccessToken>(text);
            if (acc.expires_in > 0)
            {
                acc.ExpiresTime = Utility.TimeSpan() + (acc.expires_in / 5);
                Tokens[appid] = acc;
                return acc.access_token;
            }
            Data.Utility.Error("AccessToken", appid, appSecret, text);
            return null;
            //if(Tokens
        }

        public static System.Collections.Hashtable Submit(string accessToken, String method, string json, bool check)
        {
            string text = String.Empty;
            var sb = new StringBuilder();
            var ls = method.IndexOf("&");
            var end = "";
            if (ls > -1)
            {
                end = method.Substring(ls);
                method = method.Substring(0, ls);

            }
            sb.AppendFormat("https://api.weixin.qq.com/{0}?access_token={1}{2}", method, accessToken, end);

            if (String.IsNullOrEmpty(json))
            {
                var uri = new Uri(sb.ToString());
                text = new Uri(sb.ToString()).WebRequest().Get().ReadAsString();// System.Text.Encoding.UTF8.GetString(webc.DownloadData(sb.ToString()));
            }
            else
            {
                switch (json[0])
                {
                    case '&':
                        sb.Append(json);
                        text = new Uri(sb.ToString()).WebRequest().Get().ReadAsString();
                        break;
                    case '[':
                    case '{':
                        text = new Uri(sb.ToString()).WebRequest().Post(new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8)).ReadAsString();// ;// System.Text.Encoding.UTF8.GetBytes(json)));
                        break;
                    default:
                        if (json.StartsWith("http://") || json.StartsWith("https://"))
                        {
                            var curl = UMC.Net.CURL.Create();

                            curl.AddFile("media", new Uri(json));
                            text = curl.Post(new Uri(sb.ToString()));

                        }
                        else
                        {
                            return null;
                        }
                        break;
                }
            }

            var data = UMC.Data.JSON.Deserialize<System.Collections.Hashtable>(text);
            if (data.ContainsKey("errcode") && String.Equals(data["errcode"].ToString(), "0") == false)
            {
                Data.Utility.Debug("WeiXin", sb.ToString(), json, text);
            }
            return data;
        }
        public static string GetCardAPITicket(Uri uri)
        {

            var provdier = UMC.Data.Reflection.GetDataProvider("payment", "Wxpay");
            string appId = provdier["appid"];
            string appSecret = provdier["appSecret"];


            Hashtable hash = null;
            if (uri.Host.StartsWith(appId))
            {
                hash = Submit("cgi-bin/ticket/getticket", "&type=wx_card");
            }
            else
            {

                //var webc = new System.Net.WebClient();
                var text = new Uri(String.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=wx_card"
                    , GetAccessToken(appId, appSecret))).WebRequest().Get().ReadAsString();
                hash = Data.JSON.Deserialize(text) as Hashtable;
            }

            if (hash.ContainsKey("ticket"))
            {
                var acc = new AccessToken();
                acc.access_token = hash["ticket"] as string;
                acc.expires_in = Data.Utility.IntParse(hash["expires_in"] as string, 0);
                acc.ExpiresTime = Utility.TimeSpan() + (acc.expires_in / 5);

                return acc.access_token;
            }
            else
            {
                Data.Utility.Error("APITicket", uri.AbsoluteUri, "接口异常不能获取微信卡券Ticket");
            }
            return null;
        }
        public static System.Collections.Hashtable Submit(String method, string json, string appId = "")
        {
            return null;
        }
        public static string GetJSAPI_Ticket(Uri uri)
        {
            var provdier = UMC.Data.Reflection.GetDataProvider("payment", "Wxpay");
            string appId = provdier["appid"];
            string appSecret = provdier["appSecret"];


            Hashtable hash = null;
            if (uri.Host.StartsWith(appId))
            {
                hash = Submit("cgi-bin/ticket/getticket", "&type=jsapi");
            }
            else
            {
                //var webc = new System.Net.WebClient();
                var text = new Uri(String.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi"
                    , GetAccessToken(appId, appSecret))).WebRequest().Get().ReadAsString();
                hash = Data.JSON.Deserialize(text) as Hashtable;
            }
            if (hash.ContainsKey("ticket"))
            {
                var acc = new AccessToken();
                acc.access_token = hash["ticket"] as string;
                acc.expires_in = Data.Utility.IntParse(hash["expires_in"] as string, 0);
                acc.ExpiresTime = Utility.TimeSpan() + (acc.expires_in / 5);
                return acc.access_token;
            }
            return null;
            //if(Tokens
        }
        public static Hashtable POST(string method, object value)
        {
            var webr = UMC.Data.WebResource.Instance();
            if (value is string)
            {
                return Submit(method, value as string);
            }
            else
            {
                return Submit(method, Data.JSON.Serialize(value));
            }
        }

        public class OAuth : UMC.Net.CURL
        {
            const string domain = "https://api.weixin.qq.com/";
            string Path
            {
                get;
                set;
            }
            public OAuth()
            {
            }

            public string POST(string path)
            {
                this.Path = path;
                return this.Send(this.GetUrl(), "POST");
            }
            public Hashtable PostJSON(string path)
            {
                this.Path = path;

                return this.JSONEncode(this.Send(this.GetUrl(), "POST")) as Hashtable;

            }
            public string GET(string path)
            {
                this.Path = path;
                return this.Send(this.GetUrl(), "GET");
            }
            public Hashtable GetJSON(string path)
            {
                this.Path = path;
                return this.JSONEncode(this.Send(this.GetUrl(), "GET")) as Hashtable;

            }
            Uri GetUrl()
            {
                return new Uri(domain + Path);
            }
            public string Send(string path, string method)
            {
                this.Path = path;
                return this.Send(this.GetUrl(), method);
            }
            Hashtable JSONEncode(string jsonStr)
            {
                int i2 = jsonStr.IndexOf('{');
                int i = jsonStr.IndexOf('(');
                if (i < i2 && i > -1)
                {
                    return UMC.Data.JSON.Deserialize(jsonStr, ref i) as Hashtable;
                }
                return UMC.Data.JSON.Deserialize(jsonStr) as Hashtable;
            }
        }
        public virtual int AccountType
        {
            get { return ACCOUNT_KEY; }
        }

    }

}