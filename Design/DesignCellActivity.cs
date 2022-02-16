using System;
using System.Collections;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    class DesignCellActivity : DesignClickActivity
    {
        WebMeta Text()
        {
            var webr = UMC.Data.WebResource.Instance();
            var data = new UMC.Web.WebMeta().Put("text", "插入文字");
            var cell = UICell.Create("CMSText", data);
            return new UMC.Web.WebMeta().Cell(cell);

        }
        WebMeta Image(WebRequest request)
        {
            var media_id = this.AsyncDialog("media_id", m =>
            {
                var f = Web.UIDialog.CreateDialog("File");
                f.Config.Put("Submit", new UIClick(new UMC.Web.WebMeta(request.Arguments.GetDictionary()).Put("media_id", "Value"))
                {
                    Command = request.Command,
                    Model = request.Model
                });
                return f;
            });
            var url = new Uri(media_id);
            var urlKey = String.Format("UserResources/{0:YYMMDD}/{1}{2}", DateTime.Now, UMC.Data.Utility.TimeSpan(), url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('/')));
            var webr = UMC.Data.WebResource.Instance();


            var domain = webr.WebDomain();
            var posmata = new UMC.Web.WebMeta();
            var cell = UMC.Web.UICell.Create("CMSImage", posmata);

            posmata.Put("src", String.Format("{0}{1}", domain, urlKey));

            webr.Transfer(url, urlKey);
            cell.Style.Padding(10);
            return new UMC.Web.WebMeta().Cell(cell);

        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Key = this.AsyncDialog("Key", "WebResource");
            var UI = this.AsyncDialog("UI", "none");
            var Type = this.AsyncDialog("Type", gKey =>
            {
                var seett = new Web.UISheetDialog();
                seett.Title = "插入";
                var click = new UIClick("UI", UI, "Key", "WebResource", "Type", "Image") { Command = request.Command, Model = request.Model };
                click.Text = "插入图片";
                seett.Options.Add(click);
                click = new UIClick("UI", UI, "Key", "WebResource", "Type", "Text") { Command = request.Command, Model = request.Model };
                click.Text = "插入文字";
                seett.Options.Add(click);
                return seett;

            });
            switch (Type)
            {
                case "Image":

                    this.Context.Send(new UMC.Web.WebMeta().UIEvent(Key, UI, Image(request)), true);
                    break;
                case "Text":
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent(Key, UI, Text()), true);
                    break;
            }
        }
    }
}