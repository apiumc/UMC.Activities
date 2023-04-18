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


        public const int ACCOUNT_KEY = 12;

        public static string AccessToken(String appid)
        {
            var data = Data.ConfigurationManager.DataCache(UMC.Data.Utility.Guid(appid, true).Value, "AccessToken", 6000, (k, v, h) =>
            {
                Provider provider = Reflection.Configuration("account")[appid];
                if (provider != null)
                {
                    string appSecret = provider["appsecret"];// as string;

                    var text = Get(String.Format("cgi-bin/gettoken?corpid={0}&corpsecret={1}", appid, appSecret));

                    var value = Data.JSON.Deserialize(text) as Hashtable;
                    if (value.ContainsKey("access_token"))
                    {
                        return value;
                    }
                }
                return new Hashtable();
            });
            return data["access_token"] as string;

        }
        public static string AccessToken()
        {
            var login = Reflection.Configuration("account");//.Providers.GetEnumerator();
            for (var i = 0; i < login.Count; i++)// .MoveNext())
            {
                Provider provider = login[i];// (Provider)login.Current;
                if (provider["type"] == "wxwork")
                {
                    AccessToken(provider.Name);
                }
            }
            return null;

        }



        public static String Get(String pathQuery)
        {
            return UMC.Net.APIProxy.Wxwork(pathQuery).WebRequest().Get().ReadAsString();

        }
        public static String Post(String pathQuery, String text)
        {
            return UMC.Net.APIProxy.Wxwork(pathQuery).WebRequest().Post(text).ReadAsString();


        }
        public static String Post(String pathQuery, WebMeta data)
        {

            return UMC.Net.APIProxy.Wxwork(pathQuery).WebRequest().Post(data)
                       .ReadAsString();
        }
    }

}