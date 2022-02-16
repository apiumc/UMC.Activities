using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class UIConfigActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var webRes = UMC.Data.WebResource.Instance();

            var hask = new System.Collections.Hashtable();

            hask["src"] = new Uri(request.Url, webRes.ResolveUrl(Data.WebResource.ImageResource)).AbsoluteUri;



            response.Redirect(hask);

        }

    }
}