
using System;
using UMC.Web;
namespace UMC.Activities
{ 
    class DesignImageActivity : DesignClickActivity
    {


        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var UI = this.AsyncDialog("UI", "none");
            var section = this.AsyncDialog("section", "-1");
            var row = this.AsyncDialog("row", "-1");

            var Type = this.AsyncDialog("Type", g =>
            {
                var shett = new Web.UISheetDialog() { Title = "图片操作" };
                shett.Options.Add(new UIClick(new UMC.Web.WebMeta(request.Arguments.GetDictionary()).Put("Type", "Click")) { Model = request.Model, Command = request.Command, Text = "点击连接" });
                shett.Options.Add(new UIClick(new UMC.Web.WebMeta(request.Arguments.GetDictionary()).Put("Type", "Reset")) { Model = request.Model, Command = request.Command, Text = "更换图片" });
                shett.Options.Add(new UIClick(new UMC.Web.WebMeta(request.Arguments.GetDictionary()).Put("Type", "Del")) { Model = request.Model, Command = request.Command, Text = "移除图片" });
                return shett;
            });
            switch (Type)
            {
                case "Reset":

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

                    webr.Transfer(url, urlKey);
                    var posmata = new UMC.Web.WebMeta();
                    posmata.Put("src", String.Format("{0}{1}", domain, urlKey));
                    var vale = new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "VALUE").Put("reloadSinle", true).Put("value", posmata);
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, vale), true);
                    break;
                case "Del":
                    var dvale = new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "DEL").Put("reloadSinle", true).Put("value", new UMC.Web.WebMeta());
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, dvale), true);
                    break;
                case "Click":
                    var click = this.Click(new UIClick());
                    var posmata2 = new UMC.Web.WebMeta();
                    posmata2.Put("click", click);
                    this.Prompt("图片点击设置成功", false);
                    this.Context.Send("Click", false);
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "VALUE")
                        .Put("value", posmata2)), true);
                    break;
            }

        }

    }

}