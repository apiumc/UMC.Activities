

using System;
using System.Linq;
using System.Collections.Generic;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
using UMC.Web.UI;
namespace UMC.Activities
{
    public class UIDesigner
    {
        bool _editer;

        public UIDesigner(bool editer)
        {
            _editer = editer;


        }

        public const int


                StoreDesignTypeCaption = 1,
                StoreDesignTypeProduct = 2,
                StoreDesignTypeBanners = 4,
                StoreDesignTypeTitleDesc = 128,
                StoreDesignTypeItems = 16,
                StoreDesignTypeIcons = 64,
                StoreDesignTypeItem = 32,
                StoreDesignTypeCustom = 256,
                StoreDesignTypeProducts = 512,
                StoreDesignTypeDiscounts = 1024,
                StoreDesignTypeDiscount = 2048,
                StoreDesignType = 4096;


        void Sliders(UISlider slider, Guid parentId, String type, List<PageItem> baners)
        {
            var webr = UMC.Data.WebResource.Instance();
            foreach (var b in baners)
            {

                var src = webr.ResolveUrl(String.Format("{1}{0}/1/0.jpg!{3}?{2}", b.Id, UMC.Data.WebResource.ImageResource, TimeSpan(b.ModifiedDate), type));
                if (_editer)
                {
                    slider.Add(new UIClick(new UMC.Web.WebMeta().Put("Id", b.Id)) { Command = "Item", Model = "Design" }, src);
                }
                else
                {
                    if (String.IsNullOrEmpty(b.Click) == false)
                    {
                        slider.Add(UMC.Data.JSON.Deserialize<UIClick>(b.Click), src);
                    }
                    else
                    {
                        slider.Add(src);
                    }

                }
            }
            if (slider.Count == 0 && _editer)
            {
                slider.Add(new UIClick(parentId.ToString()).Send("Design", "Item"));

            }

        }
        //
        void Sliders(PageItem parent, List<PageItem> baners, UISection U)
        {
            if (baners.Count > 0)
            {
                WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(parent.Data) ?? new UMC.Web.WebMeta();


                var type = config["type"];
                var sType = "slider";
                var slider2 = new UISlider(config.ContainsKey("auto"));
                switch (type)
                {
                    case "Small":
                        slider2.Small();
                        sType = "4-1";
                        break;
                    case "Square":
                        slider2.Square();
                        sType = "350";
                        break;
                    default:
                        type = "slider";
                        break;
                }

                Sliders(slider2, parent.Id.Value, sType, baners);


                int[] paddings = UIStyle.Padding(config);
                if (paddings.Length > 0)
                {
                    slider2.Style.Padding(paddings);
                }
                U.Add(slider2);
            }
            else if (_editer)
            {

                UIDesc desc = new UIDesc("\ue907");
                desc.Click(new UIClick(parent.Id.ToString())
                                    .Send("Design", "Item"));
                desc.Desc("{desc}\r\n配置横幅栏");
                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38));
                U.Add(desc);

            }
        }
        public static List<WebMeta> GetBanner(Guid appkey, bool editer, params Guid[] ids)
        {
            var maxItems = DataFactory.Instance().DesignItems(appkey, ids[0], ids).ToList();


            var webr = UMC.Data.WebResource.Instance();

            var lis = new List<WebMeta>();
            var banners = maxItems.FindAll(g => g.Type == UIDesigner.StoreDesignTypeBanners);

            maxItems.RemoveAll(g =>
            {
                switch (g.Type.Value)
                {
                    case UIDesigner.StoreDesignTypeCustom:
                    case UIDesigner.StoreDesignTypeItem:
                        return false;
                }
                return true;
            });
            List<PageItem> items = new List<PageItem>();
            foreach (var did in ids)
            {
                items = maxItems.FindAll(g => g.design_id == did);
                if (items.Count > 0)
                {
                    break;
                }

            }
            var designItem = banners.Find(g => g.design_id == ids[0]);
            if (editer)
            {
                if (designItem == null)
                {
                    designItem = new PageItem
                    {
                        Id = Guid.NewGuid(),
                        Type = UIDesigner.StoreDesignTypeBanners,
                        for_id = Guid.Empty,
                        design_id = ids[0]
                    };

                    banners.Add(designItem);
                    DataFactory.Instance().Put(designItem);
                }
            }
            foreach (var b in items)
            {
                var pms = JSON.Deserialize<WebMeta>(b.Data) ?? new UMC.Web.WebMeta();
                pms.Put("id", b.Id);
                pms.Put("click", Click(b, designItem, editer));
                if (editer)
                {
                    pms.Put("design", true);
                }
                pms.Put("src", webr.ResolveUrl(String.Format("{1}{0}/1/0.jpg?{2}", b.Id, UMC.Data.WebResource.ImageResource, Utility.TimeSpan(b.ModifiedDate ?? DateTime.Now))));
                lis.Add(pms);
            }
            if (items.Count == 0)
            {
                if (editer)
                {
                    lis.Add(new UMC.Web.WebMeta().Put("design", true).Put("click", new UIClick(new UMC.Web.WebMeta().Put("Id", designItem.Id)) { Command = "Item", Model = "Design" }));

                }
            }
            return lis;//
        }

        static UIClick Click(PageItem item, PageItem design, bool editer)
        {
            if (editer)
            {
                if (design.design_id == item.design_id)
                {
                    return new UIClick(new UMC.Web.WebMeta().Put("Id", item.Id.ToString())) { Command = "Item", Model = "Design" };
                }
                else
                {
                    return new UIClick(new UMC.Web.WebMeta().Put("Id", design.Id.ToString())) { Command = "Item", Model = "Design" };

                }
            }
            else
            {
                return JSON.Deserialize<UIClick>(item.Click);
            }
        }

        void Icons(Guid parentId, List<PageItem> baners, UISection U)
        {
            List<UIEventText> list = new List<UIEventText>();
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            foreach (PageItem b in baners)
            {
                UIEventText slider = new UIEventText(b.ItemName);
                if (String.IsNullOrEmpty(b.Data) == false)
                {
                    WebMeta s = UMC.Data.JSON.Deserialize<WebMeta>(b.Data);

                    slider.Icon(s.Get("icon"), s.Get("color"));

                }
                else
                {
                    slider.Src(webr.ImageResolve(b.Id.Value, "1", 4) + "?" + TimeSpan(b.ModifiedDate));

                }
                slider.Click(this.Click(b));

                list.Add(slider);

            }
            if (list.Count > 0)
            {
                var ic = new UMC.Web.UI.UIIcon();
                ic.Style.Name("icon", new UIStyle().Font("wdk").Size(24));

                ic.Add(list.ToArray());
                //U.AddIcon(new UIStyle().Name("icon", new UIStyle().Font("wdk").Size(24)), list.ToArray());
            }
            else if (_editer)
            {
                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置图标栏");
                desc.Click(new UIClick(parentId.ToString())
                        .Send("Design", "Item"));

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38));
                U.Add(desc);

            }

        }

        void Items(PageItem parent, List<PageItem> baners, UISection U)
        {
            Guid parentId = parent.Id.Value;
            //List<UIItem> list = new List<UIItem>();
            var item = new UIItems();
            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();
            for (int i = 0; i < baners.Count && i < 4; i++)
            {
                PageItem b = baners[i];
                WebMeta icon = UMC.Data.JSON.Deserialize<WebMeta>(b.Data) ?? new UMC.Web.WebMeta();
                item.Add(icon);
                icon.Put("click", this.Click(b));
                String t = "100";
                switch (baners.Count)
                {
                    case 1:
                        t = "4-1";
                        break;
                    case 2:
                        t = "2-1";
                        break;
                    case 3:
                        if (i == 0)
                        {
                            t = "2-1";
                        }
                        break;
                }

                icon.Put("src", String.Format("{0}!{1}?{2}", webr.ImageResolve(b.Id.Value, "1", 0), t, TimeSpan(b.ModifiedDate)));
                //list.Add(slider);

            }
            if (item.Count > 0)
            {
                U.Add(item);

            }
            else if (_editer)
            {
                ;
                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置分块栏");

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38).Click(new UIClick(parentId.ToString())
                        .Send("Design", "Item")));
                U.Add(desc);
            }
        }


        void TitleDesc(PageItem parent, List<PageItem> items, UISection U)
        {

            UMC.Data.WebResource webr = UMC.Data.WebResource.Instance();


            //WebMeta config = Utility.isNull(UMC.Data.JSON.deserialize(parent.Data, WebMeta.class), new UMC.Web.WebMeta());
            WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(parent.Data) ?? new UMC.Web.WebMeta();

            int rows = UMC.Data.Utility.IntParse(config.Get("Total"), 1);
            if (rows <= 1)
            {
                int[] padding = UIStyle.Padding(config);
                foreach (PageItem i in items)
                {
                    UICell tdesc = this.TitleDesc(config, i, "cms1", webr);
                    if (padding.Length > 0)
                        tdesc.Style.Padding(padding);
                    U.Add(tdesc);
                }
            }
            else
            {
                int m = 0;
                String hide = config.Get("Hide") ?? "";
                if (hide.Contains("HideTitle"))
                {
                    m |= 1;
                }
                if (hide.Contains("HideDesc"))
                {
                    m |= 2;
                }
                if (hide.Contains("HideLeft"))
                {
                    m |= 4;
                }
                if (hide.Contains("HideRight"))
                {
                    m |= 8;
                }

                int len = items.Count;

                for (int i = 0; (i + rows - 1) < len; i = i + rows)
                {
                    List<WebMeta> ls = new List<WebMeta>();//<>();
                    for (int c = 0; c < rows; c++)
                    {
                        UICell p = TitleDesc(config, items[i + c], "350", webr);
                        ls.Add(new UMC.Web.WebMeta().Put("value", p.Data).Put("format", p.Formats).Put("style", p.Style));

                    }
                    UICell desc = UICell.Create("ItemsTitleDesc", new UMC.Web.WebMeta().Put("items", ls.ToArray()).Put("total", rows).Put("show", m));
                    int[] paddings = UIStyle.Padding(config);
                    if (paddings.Length > 0)
                    {
                        desc.Style.Padding(paddings);
                    }
                    U.Add(desc);
                }
                int total = len % rows;

                if (total > 0)
                {
                    List<WebMeta> ls = new List<WebMeta>();
                    for (int c = total; c > 0; c--)
                    {
                        UICell p = TitleDesc(config, items[len - c], "350", webr);
                        ls.Add(new UMC.Web.WebMeta().Put("value", p.Data).Put("format", p.Formats).Put("style", p.Style));

                    }

                    UICell desc = UICell.Create("ItemsTitleDesc", new UMC.Web.WebMeta().Put("items", ls.ToArray()).Put("total", rows).Put("show", m));
                    int[] paddings = UIStyle.Padding(config);
                    if (paddings.Length > 0)
                    {
                        desc.Style.Padding(paddings);
                    }
                    U.Add(desc);

                }


            }
            if (items.Count == 0 && _editer)
            {

                UIDesc desc = new UIDesc("\ue907");
                desc.Desc("{desc}\r\n配置图文栏");

                desc.Style.AlignCenter().Name("desc", new UIStyle().Font("wdk").Size(38).Click(new UIClick(parent.Id.ToString())
                        .Send("Design", "Item")));
                U.Add(desc);

            }
        }

        UIImageTitleDescBottom TitleDesc(WebMeta config, PageItem item, String img, UMC.Data.WebResource webr)
        {

            //WebMeta data = Utility.isNull(UMC.Data.JSON.deserialize(item.Data, WebMeta.class), new UMC.Web.WebMeta());
            WebMeta data = UMC.Data.JSON.Deserialize<WebMeta>(item.Data) ?? new UMC.Web.WebMeta();

            int m = 0;
            String hide = config.Get("Hide") ?? "";

            if (hide.Contains("HideTitle"))
            {
                m |= 1;
                data.Remove("title");
            }
            if (hide.Contains("HideDesc"))
            {
                m |= 2;
                data.Remove("desc");
            }
            if (hide.Contains("HideLeft"))
            {
                m |= 4;
                data.Remove("left");
            }
            if (hide.Contains("HideRight"))
            {
                m |= 8;
                data.Remove("right");
            }
            data.Put("show", m);
            String src = (String.Format("{0}!{1}?{2}", webr.ImageResolve(item.Id.Value, "1", 0), img, TimeSpan(item.ModifiedDate)));
            //        list.add(slider);

            //
            UIImageTitleDescBottom btm = new UIImageTitleDescBottom(data, src);
            btm.Click(this.Click(item));
            var left = data["left"];
            if (String.IsNullOrEmpty(left) == false)
            {
                var p = @"\d+\.?\d{0,2}";
                int i = -1;
                var t = System.Text.RegularExpressions.Regex.Replace(left, p, dr =>
                {
                    i++;
                    switch (i)
                    {
                        case 0:
                            data.Put("price", dr.Value);
                            return "￥{1:price} ";
                        case 1:
                            data.Put("orgin", dr.Value);
                            return " {orgin}";

                    }
                    return dr.Value;
                }, System.Text.RegularExpressions.RegexOptions.Multiline);
                btm.Left(t);
                btm.Style.Name("price", new UIStyle().Size(16).Color(0xdb3652)).Name("unit", new UIStyle().Size(12).Color(0x999)).Name("orgin", new UIStyle().Color(0x999).Size(12).DelLine());

            }

    ;
            return btm;

        }

        public static UIClick Click(PageItem item, bool editer)
        {
            if (editer)
            {

                return new UIClick(item.Id.ToString()).Send("Design", "Item");
            }
            else
            {
                return JSON.Deserialize<UIClick>(item.Click);
            }
        }
        public static int TimeSpan(DateTime? date)
        {
            return date.HasValue ? UMC.Data.Utility.TimeSpan(date.Value) : 0;
        }

        UIClick Click(PageItem item)
        {

            return Click(item, _editer);
        }

        private UISection Section(UISection Us, List<PageItem> items)
        {
            List<PageItem> groups = items.FindAll(g => g.for_id == Guid.Empty);

            PageItem b = groups.Find(g => g.Type == StoreDesignTypeBanners);


            if (b != null)
            {
                Sliders(b, items.FindAll(it => it.for_id == b.Id), Us);
            }


            if (b != null)
            {
                groups.Remove(b);
            }
            foreach (PageItem bp in groups)
            {
                UISection use = Us;
                if (Us.Length > 0)
                {
                    use = Us.NewSection();
                }
                switch (bp.Type ?? 0)
                {
                    case StoreDesignTypeBanners:
                        Sliders(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeIcons:
                        //Icons(bp.Id, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        Icons(bp.Id.Value, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeItems:

                        //Items(bp, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        Items(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeTitleDesc:

                        //TitleDesc(bp, Utility.findAll(items, it->it.for_id.compareTo(bp.Id) == 0), use);
                        TitleDesc(bp, items.FindAll(it => it.for_id == bp.Id), use);
                        break;
                    case StoreDesignTypeProducts:
                    case StoreDesignTypeDiscounts:
                        break;
                    case StoreDesignTypeCaption:

                        //WebMeta config = Utility.isNull(UMC.Data.JSON.deserialize(bp.Data, WebMeta), new UMC.Web.WebMeta());
                        WebMeta config = UMC.Data.JSON.Deserialize<WebMeta>(bp.Data) ?? new UMC.Web.WebMeta();
                        if ("Hide".Equals(config.Get("Show")))
                        {
                            if (_editer)
                            {
                                UITitleMore more = new UITitleMore(bp.ItemName).More("已隐藏{3:more}");
                                more.Style.Name("more", new UIStyle().Color(0xc00));

                                use.Add(more.Click(this.Click(bp)));
                            }
                        }
                        else
                        {
                            UITitleMore more = new UITitleMore(bp.ItemName).Click(this.Click(bp));

                            more.Style.Padding(UIStyle.Padding(config));
                            use.Add(more);
                        }
                        List<PageItem> groups2 = items.FindAll(it => it.for_id == (bp.Id));// items.FindAll(it = > it.for_id == bp.Id);
                        foreach (PageItem bp2 in groups2)
                        {
                            switch (bp2.Type)
                            {
                                case StoreDesignTypeBanners:
                                    Sliders(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeIcons:
                                    //Icons(bp2.Id, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    Icons(bp2.Id.Value, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeItems:
                                    //Items(bp2, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    Items(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                                case StoreDesignTypeTitleDesc:

                                    //TitleDesc(bp2, Utility.findAll(items, it->it.for_id.compareTo(bp2.Id) == 0), use);
                                    TitleDesc(bp2, items.FindAll(it => it.for_id == (bp2.Id)), use);
                                    break;
                            }
                        }


                        break;


                }
            }
            return Us;
        }



        public UISection Section(String title, Guid appKey, Guid design_id)
        {

            UISection Us = String.IsNullOrEmpty(title) ? UISection.Create() : UISection.Create(new UITitle(title));
            return this.Section(Us, appKey, design_id);
        }

        public UISection Section(UISection Us, Guid appKey, Guid design_id)
        {
            //   List<Design_Item> items = new List<Design_Item>();

            var items = DataFactory.Instance().DesignItems(appKey, design_id).OrderBy(r => r.Seq ?? 0).ToList();
            //Database.Instance().ObjectEntity<Design_Item>()
            //        .Where.And().Equal(new Design_Item() { design_id = (design_id) })
            //        .Entities.Order.Asc(new Design_Item() { Seq = 0 }).Entities
            //        .Query(dr => items.Add(dr));


            return this.Section(Us, items);
        }

    }
}