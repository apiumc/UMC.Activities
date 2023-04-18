

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

        public static bool IsDesign(WebContext context)
        {
            if (context.Request.IsCashier)
            {
                return "true".Equals(context.Token.Get("UIDesign"));

            }
            return false;
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            _IsDesign = IsDesign(this.Context);

            List<Guid> ids = new List<Guid>();
            List<String> strIds = new List<string>();

            String strs = this.AsyncDialog("Id", g => this.DialogValue("none"));
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

            List<Guid> pids = new List<Guid>();

            List<PageItem> items;

            if (ids.Count == 1)
            {
                items = DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, ids[0], new Guid[0]).ToList();
            }
            else
            {
                items = DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, ids[0], ids.ToArray()).Where(r => r.Type == UIDesigner.StoreDesignTypeCustom).ToList();//[0]).ToList();


            }

            List<WebMeta> lis = new List<WebMeta>();

            var webr = UMC.Data.WebResource.Instance();
            if (strIds.Count > 0)
            {
                String config = this.AsyncDialog("Config", g => this.DialogValue("none"));

                for (int i = 0; i < strIds.Count; i++)
                {
                    Guid cid = ids[i];
                    PageItem item = items.Find(k => k.Id == cid);

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
                foreach (PageItem b in items)
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