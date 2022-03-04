using System;
using System.Collections.Generic;
using System.Text;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class DesignKeyItemActivity : Web.WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = this.Context.Token.Identity(); // UMC.Security.Identity.Current; 

            var strId = this.AsyncDialog("Id", g => this.DialogValue(Guid.NewGuid().ToString()));
            var itemId = Data.Utility.Guid(strId, true);//, true).Value;



            //var itemsEntity = Database.Instance().ObjectEntity<Design_Item>() 
            //.Where.And().Equal(new Design_Item { Id = itemId });

            var item = DataFactory.Instance().DesignItem(itemId.Value);
            var name = this.AsyncDialog("Name", g =>
            {
                return this.DialogValue("Src");
            });


            var webr = UMC.Data.WebResource.Instance();
            var desc = this.AsyncDialog("Desc", g =>
            {
                var size = this.AsyncDialog("Size", sg =>
                {
                    return this.DialogValue("none");
                });
                var fm = new Web.UIFormDialog() { Title = "上传图片" };


                if (String.Equals(size, "none"))
                {
                    size = "注意图片尺寸";
                }
                else
                {

                    size = String.Format("图片尺寸:{0}", size);
                }

                fm.AddFile(size, "Desc", webr.ResolveUrl(String.Format("{0}{1}/1/0.jpg!100", UMC.Data.WebResource.ImageResource, itemId)))

                  .Command("Design", "Picture", new UMC.Web.WebMeta().Put("id", itemId).Put("seq", "1", "type", "jpg"));


                return fm;
            });
            var ite = new Design_Item
            {
                Type = UIDesigner. StoreDesignTypeCustom,
                ModifiedDate = DateTime.Now,
                Id = itemId,
                Seq = 0
            };
            var data = UMC.Data.JSON.Deserialize<WebMeta>(item != null ? item.Data : "") ?? new UMC.Web.WebMeta();
            if (name != "Src")
            {
                data.Put(name, desc);

            }
            if (item == null)
            {
                ite.design_id = UMC.Data.Utility.Guid("UISettings", true); 
            }
            ite.Data = UMC.Data.JSON.Serialize(data);

            //itemsEntity.Entities.IFF(e => e.Update(ite) == 0, e =>
            //{
            //    ite.design_id = UMC.Data.Utility.Guid("UISettings", true);
            //    e.Insert(ite);
            //});
            DataFactory.Instance().Put(ite);

            this.Context.Send(new UMC.Web.WebMeta().Put("type", "DesignItem")
                .Put("Id", strId).Put("data", data)
                .Put("name", name).Put("data", data)
                .Put("src", webr.ResolveUrl(String.Format("{1}{0}/1/0.jpg?{2}", ite.Id, UMC.Data.WebResource.ImageResource, this.TimeSpan(ite.ModifiedDate)))), true);

        }
        int TimeSpan(DateTime? date)
        {
            return date.HasValue ? UMC.Data.Utility.TimeSpan(date.Value) : 0;
        }
    }
}
