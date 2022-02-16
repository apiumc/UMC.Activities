

using System;
using System.Linq;
using System.Collections.Generic;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{

    public class DesignItemsActivity : WebActivity
    {
        private bool _IsDesign;

        public static bool IsDesign(WebRequest request)
        {
            if (request.IsCashier)
            {
                return "true".Equals(UMC.Security.AccessToken.Get("UIDesign"));//, "true");

            }
            return false;
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            _IsDesign = IsDesign(request);

            List<Guid> ids = new List<Guid>();
            List<String> strIds = new List<string>();

            String strs = this.AsyncDialog("Id", g => this.DialogValue("none"));//, true).Value;
            if (strs.IndexOf(',') > -1)
            {
                foreach (String s in strs.Split(','))
                {
                    if (String.IsNullOrEmpty(s) == false)
                    {
                        ids.Add(Utility.Guid(s, true).Value);
                        strIds.Add(s);
                    }
                }
            }
            else
            {
                ids.Add(Utility.Guid(strs, true).Value);

            }

            List<Guid> pids = new List<Guid>(); ;// new LinkedList<>();

            List<Design_Item> items;

            //var itemsEntity = Database.Instance().ObjectEntity<Design_Item>();
            if (ids.Count == 1)
            {
                items = DataFactory.Instance().DesignItems(ids[0]).ToList();
                //itemsEntity.Where.And().Equal(new Design_Item() { design_id = ids[0] });

            }
            else
            {
                items = DataFactory.Instance().DesignItems(ids[0], ids.ToArray()).Where(r => r.Type == UIDesigner.StoreDesignTypeCustom).ToList();//[0]).ToList();

                //itemsEntity.Where.And().In(new Design_Item() { design_id = ids[0] }, ids.ToArray())
                //    .And().Equal(new Design_Item { Type = UIDesigner.StoreDesignTypeCustom });


            }
            //itemsEntity.Order.Asc(new Design_Item { Seq = 0 }).Entities.Query(dr => items.Add(dr));


            List<WebMeta> lis = new List<WebMeta>();

            var webr = UMC.Data.WebResource.Instance();
            if (strIds.Count > 0)
            {
                String config = this.AsyncDialog("Config", g => this.DialogValue("none"));

                for (int i = 0; i < strIds.Count; i++)// var b in items)
                {
                    Guid cid = ids[i];
                    Design_Item item = items.Find(k => k.Id == cid);// Utility.find(items, g->g.Id.compareTo(cid) == 0);

                    if (item != null)
                    {
                        WebMeta pms = JSON.Deserialize<WebMeta>(item.Data);
                        pms.Put("id", strIds[i]);
                        if (_IsDesign)
                        {
                            pms.Put("design", true);
                            if (config.Equals("UISEO"))
                            {
                                pms.Put("click", new UIClick(new UMC.Web.WebMeta().Put("Id", item.Id).Put("Config", config))
                                        .Send("Design", "Custom"));
                            }
                            else
                            {
                                pms.Put("click", UIDesigner.Click(item, true));
                            }


                        }
                        else
                        {
                            pms.Put("click", UIDesigner.Click(item, false));
                        }
                        pms.Put("src", webr.ImageResolve(item.Id.Value, "1", 0) + "?" + UIDesigner.TimeSpan(item.ModifiedDate));

                        lis.Add(pms);
                    }
                    else
                    {
                        if (_IsDesign)
                        {
                            lis.Add(new UMC.Web.WebMeta().Put("design", true).Put("id", strIds[i]).Put("click", new UIClick(new UMC.Web.WebMeta()
                                .Put("Id", Utility.Guid(strIds[i], true).ToString(), "Config", config))
                                    .Send("Design", "Custom")));


                        }
                    }
                }
            }
            else
            {

                items.RemoveAll(g =>
              {
                  switch (g.Type)
                  {
                      case UIDesigner.StoreDesignTypeCustom:
                      case UIDesigner.StoreDesignTypeItem:
                          return false;
                  }
                  return true;
              });
                foreach (Design_Item b in items)
                {
                    WebMeta pms = JSON.Deserialize<WebMeta>(b.Data);
                    pms.Put("id", b.Id);
                    pms.Put("click", UIDesigner.Click(b, _IsDesign));
                    if (_IsDesign)
                    {
                        pms.Put("design", true);
                    }
                    pms.Put("src", webr.ImageResolve(b.Id.Value, "1", 0) + UIDesigner.TimeSpan(b.ModifiedDate));
                    lis.Add(pms);
                }
                if (items.Count == 0)
                {
                    if (_IsDesign)
                    {
                        String config = this.AsyncDialog("Config", g => this.DialogValue(strs));
                        lis.Add(new UMC.Web.WebMeta().Put("design", true).Put("click", new UIClick(new UMC.Web.WebMeta().Put("Config", config))
                                                    .Send("Design", "Custom")));

                    }
                }
            }
            response.Redirect(lis);
        }

    }
}