using System;
using System.Collections;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{

    class DesignWebResourcActivity : UMC.Web.WebActivity
    {


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var oosr = WebResource.Instance();
            var Key = this.AsyncDialog("Key", g =>
            {
                var f = Web.UIDialog.CreateDialog("File");
                f.Config.Put("Submit", new UIClick(new WebMeta().Put("Key", "WebResource", "media_id", "Value", "UI", this.AsyncDialog("UI", "none")))
                {
                    Command = request.Command,
                    Model = request.Model
                });
                return f;
            });
            var user = Security.Identity.Current;
            var media_id = this.AsyncDialog("media_id", "none");
            if (String.Equals(media_id, "none") == false)
            {

                var url = new Uri(media_id);
                var name = url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('/') + 1);

                var urlKey = String.Format("UserResources/{0}/{1}/{2}", Utility.Parse36Encode(user.Id.Value), Utility.TimeSpan(), name);

                oosr.Transfer(url, urlKey);

                var posmata = new WebMeta();

                posmata.Put("src", String.Format("{0}{1}", oosr.WebDomain(), urlKey)).Put("name", name);
                this.Context.Send(new WebMeta().UIEvent(Key, this.AsyncDialog("UI", "none"), posmata), true);
            }
            else
            {
                var UseKey = UMC.Data.Utility.Parse36Encode(UMC.Security.Identity.Current.Id.Value);
                var sourceKey = new Uri(String.Format("{2}TEMP/{0}/{1}", UMC.Data.Utility.GetRoot(request.Url), Key, oosr.TempDomain()));

                Key = String.Format("UserResources/{0}/{1}", UseKey, Key);


                oosr.Transfer(sourceKey, Key);


                response.Redirect(new WebMeta().Put("src", String.Format("{0}{1}", oosr.WebDomain(), Key)));
            }

        }

    }
}