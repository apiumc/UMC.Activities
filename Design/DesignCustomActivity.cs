
using System;
using System.Linq;
using System.Collections.Generic;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    class DesignCustomActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var user = this.Context.Token.Identity(); // UMC.Security.Identity.Current;
            var ids = new List<Guid>();
            var itemId = Utility.Guid(this.AsyncDialog("Id", g => this.DialogValue(Guid.NewGuid().ToString())), true);//, true).Value;

            var config = this.AsyncDialog("Config", g =>
             {
                 return this.DialogValue("none");
             });

            var pids = new List<Guid>();
            var items = new List<Design_Item>();

            var size = this.AsyncDialog("Size", g =>
            {
                return this.DialogValue("none");
            });
            var item = DataFactory.Instance().DesignItem(itemId.Value);
            var webr = UMC.Data.WebResource.Instance();
            var ts = this.AsyncDialog("Setting", g =>
            {
                var fm = new UIFormDialog() { Title = "界面配置" };

                WebMeta meta = new UMC.Web.WebMeta();
                if (item != null)
                {
                    meta = UMC.Data.JSON.Deserialize<WebMeta>(item.Data) ?? meta;
                    if (String.Equals(config, "none"))
                    {
                        if (meta.ContainsKey("Config"))
                        {
                            config = meta["Config"];
                        }
                    }
                }

                if (String.Equals(config, "none"))
                {
                    this.Prompt("您配置错误");
                }
                else if (config == "UISEO")
                {
                    fm.Title = "SEO优化";
                    fm.AddTextarea("标题", "Title", meta["Title"]);
                    fm.AddTextarea("关键词", "Keywords", meta["Keywords"]);
                    fm.AddTextarea("描述", "Description", meta["Description"]);
                    return fm;
                }
                request.Arguments["Config"] = config;

                var keyIndex = config.IndexOf('.');
                if (keyIndex > -1)
                {
                    config = config.Substring(keyIndex + 1);
                }
                if (String.Equals(size, "none"))
                {
                    size = "注意图片尺寸";
                }
                else
                {

                    size = String.Format("图片尺寸:{0}", size);
                }

                var pices = DataFactory.Instance().DesignConfig(config);
                if (new List<Design_Config>(pices).Exists(dr => dr.Value == "Image" && dr.Name == "none") == false)
                {

                    fm.AddFile(size, "_Image", webr.ResolveUrl(String.Format("{0}{1}/1/0.jpg!100", UMC.Data.WebResource.ImageResource, itemId)))

                  .Command("Design", "Picture", new UMC.Web.WebMeta().Put("id", itemId).Put("seq", "1", "type", "jpg"));
                }
                UMC.Data.Utility.Each(pices, dr =>
                 {
                     if (dr.Value == "Image" && dr.Name == "none")
                     {

                     }
                     else
                     {

                         fm.AddText(dr.Name, dr.Value, meta[dr.Value]);
                     }
                 });
                if (item == null)
                {
                    var max=  DataFactory.Instance().DesignItems(UMC.Data.Utility.Guid(config, true).Value).MAX(r => r.Seq ?? 0);

                    item =new Design_Item { Seq = max + 1 };
                }

                fm.AddNumber("展示顺序", "Seq", item.Seq ?? 0);

                return fm;
            });
            var seq = UMC.Data.Utility.IntParse(ts["Seq"], 0);
            ts.Remove("Seq");
            ts.Remove("Image");
            ts["Config"] = config;
            var ite = new Design_Item
            {
                Type = UIDesigner.StoreDesignTypeCustom,
                ModifiedDate = DateTime.Now,
                Data = UMC.Data.JSON.Serialize(ts),
                Id = itemId,
                Seq = seq
            };
            if (item == null)
            {
                ite.design_id = UMC.Data.Utility.Guid(config, true);

            }
            DataFactory.Instance().Put(ite);

            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Design"), true);
        }
    }
}