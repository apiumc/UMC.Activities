using System;
using System.Collections.Generic;
using System.Collections;
using UMC.Data;
using UMC.Net;
using System.Text;
using UMC.Web;

namespace UMC.Configuration
{

    public class WeiXin
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

        public virtual int AccountType
        {
            get { return ACCOUNT_KEY; }
        }
        public static String Get(String pathQuery)
        {
            return UMC.Net.APIProxy.WeiXin(pathQuery).WebRequest().Get().ReadAsString();
        }
        public static String Post(String pathQuery, String text)
        {
            return UMC.Net.APIProxy.WeiXin(pathQuery).WebRequest().Post(text)
                   .ReadAsString();
        }
        public static String Post(String pathQuery, WebMeta data)
        {
            return UMC.Net.APIProxy.WeiXin(pathQuery).WebRequest().Post(data)
                   .ReadAsString();
        }

    }

}