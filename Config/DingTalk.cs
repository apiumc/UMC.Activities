﻿using System;
using System.Collections.Generic;
using System.Collections;
using UMC.Data;
using UMC.Net;
using System.Text;
using System.Threading.Tasks;
using UMC.Web;

namespace UMC.Configuration
{

    public class DingTalk
    {

        public const int ACCOUNT_KEY = 11;

        public static string AccessToken(String appid)
        {
            var data = Data.ConfigurationManager.DataCache(UMC.Data.Utility.Guid(appid, true).Value, "AccessToken", 6000, (k, v, h) =>
              {
                  var provider = Reflection.Configuration("account")[appid];
                  if (provider != null)
                  {
                      string appSecret = provider["appsecret"];
                      string appKey = provider["appkey"];

                      var text = Get(String.Format("gettoken?appkey={0}&appsecret={1}", appKey, appSecret));
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
                if (String.Equals(provider.Type, "dingtalk"))
                {
                    return AccessToken(provider.Name);
                }
            }
            return null;

        }
        public static Provider AppConfig()
        {
            var login = Reflection.Configuration("account");//.Providers.GetEnumerator();
            for (var i = 0; i < login.Count; i++)// .MoveNext())
            {
                Provider provider = login[i];// (Provider)login.Current;

                if (String.Equals(provider.Type, "dingtalk.app"))
                {
                    return provider;
                }
            }
            return null;
        }
        public virtual int AccountType
        {
            get { return ACCOUNT_KEY; }
        }

        public static String Get(String pathQuery)
        {
            return UMC.Net.APIProxy.DingTalk(pathQuery).WebRequest().Get().ReadAsString();
        }
        public static String Post(String pathQuery, String text)
        {
            return UMC.Net.APIProxy.DingTalk(pathQuery).WebRequest().Post(text)
                   .ReadAsString();
        }
        public static String Post(String pathQuery, WebMeta data)
        {
            return UMC.Net.APIProxy.DingTalk(pathQuery).WebRequest().Post(data)
                   .ReadAsString();
        }

    }

}