using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UMC.Data;
using UMC.Web;
using UMC.Net;

namespace UMC.Configuration
{

    /// <summary>
    /// 微信企业号
    /// </summary>
    public class Corp
    {

        /// <summary>
        /// 
        /// </summary>
        public enum Type
        {
            Text = 0,
            Image = 1,
            Location = 2,
            Link = 3,
            Event = 4,
            News = 5,
            Music = 6,
            Voice = 7

        }

        public static string JSAPITicket(String appid)
        {
            var data = Data.ConfigurationManager.DataCache(UMC.Data.Utility.Guid(appid, true).Value, "JSAPITicket", 6000, (k, v, h) =>
            {


                ;
                var text = Get(String.Format("cgi-bin/get_jsapi_ticket?access_token={0}", AccessToken(appid)));

                var value = Data.JSON.Deserialize(text) as Hashtable;
                if (value.ContainsKey("ticket"))
                {
                    return value;
                }

                return new Hashtable();
            });
            return data["ticket"] as string;

        }

        public const int ACCOUNT_KEY = 12;

        public static string AccessToken(String appid)
        {
            var data = Data.ConfigurationManager.DataCache(UMC.Data.Utility.Guid(appid, true).Value, "AccessToken", 6000, (k, v, h) =>
            {
                var login = (UMC.Data.DataFactory.Instance().Configuration("account") ?? new ProviderConfiguration()).Providers.GetEnumerator();
                while (login.MoveNext())
                {
                    Provider provider = (Provider)login.Value;
                    if (String.Equals(provider["appid"], appid))
                    {
                        string appSecret = provider["appsecret"];// as string;

                        var text = Get(String.Format("cgi-bin/gettoken?corpid={0}&corpsecret={1}", appid, appSecret));

                        var value = Data.JSON.Deserialize(text) as Hashtable;
                        if (value.ContainsKey("access_token"))
                        {
                            return value;
                        }
                    }
                }
                return new Hashtable();
            });
            return data["access_token"] as string;

        }
        public static string AccessToken()
        {
            var login = (UMC.Data.DataFactory.Instance().Configuration("account") ?? new ProviderConfiguration()).Providers.GetEnumerator();
            while (login.MoveNext())
            {
                Provider provider = (Provider)login.Value;
                if (provider["type"] == "wxwork")
                {
                    AccessToken(provider["appid"]);
                }
            }
            return null;

        }

        public static String Get(String pathQuery)
        {
            return new Uri(String.Format("https://ali.365lu.cn/wxwork/{0}", pathQuery)).WebRequest().Get().ReadAsString();

        }
        public static String Post(String pathQuery, String text)
        {
            return new Uri(String.Format("https://ali.365lu.cn/wxwork/{0}", pathQuery)).WebRequest().Post(new System.Net.Http.StringContent(text)).ReadAsString();


        }
        public static String Post(String pathQuery, WebMeta data)
        {

            return new Uri(String.Format("https://ali.365lu.cn/wxwork/{0}", pathQuery)).WebRequest().Post(new System.Net.Http.StringContent(JSON.Serialize(data)))
                       .ReadAsString();
        }
        public virtual int AccountType
        {
            get { return ACCOUNT_KEY; }
        }

    }

}