﻿
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    class DesignPageActivity : WebActivity
    {
        bool _isEditer;
        public DesignPageActivity()
        {
            this._isEditer = true;
        }
        public DesignPageActivity(bool isediter)
        {
            _isEditer = isediter;

        }

        void Design(Guid itemId)
        {

            PageItem item = DataFactory.Instance().DesignItem(itemId);

            if (item == null)
            {

                var max = DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, Guid.Empty, Guid.Empty).MAX(r => r.Seq ?? 0);

                item = new PageItem { Seq = max + 1 };
            }
            PageItem fitem = item;
            WebMeta meta = this.AsyncDialog(g =>
            {
                UIFormDialog from = new UIFormDialog();
                from.Title = "页面分类项";

                from.AddText("标题", "ItemName", fitem.ItemName);

                from.AddNumber("顺序", "Seq", fitem.Seq);

                from.Submit("确认", "Design");
                return from;
            }, "Setting");
            PageItem newItem = new PageItem()
            {
                ItemName = meta.Get("ItemName"),
                ModifiedDate = DateTime.Now,
                Seq = Utility.Parse(meta.Get("Seq"), 0)
            };
            if (item.Id.HasValue == false)
            {
                newItem.design_id = Guid.Empty;
                newItem.for_id = Guid.Empty;
                newItem.Id = Guid.NewGuid();
                newItem.Type = UIDesigner.StoreDesignType;

                newItem.AppKey = this.Context.AppKey ?? Guid.Empty;
                DataFactory.Instance().Put(newItem);
            }
            else
            {
                newItem.Id = itemId;
                DataFactory.Instance().Put(newItem);
            }
            this.Context.Send("Design", true);


        }

        void Delete(Guid uuid)
        {
            if (DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, uuid).Length > 0)
            {
                this.Prompt("设计页面有组件，先删除组件，再删除页面项");
            }

            DataFactory.Instance().Delete(new PageItem { Id = uuid });
            this.Context.Send("Design", true);
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            Guid designId = UMC.Data.Utility.Guid(this.AsyncDialog("Id", g => this.DialogValue(Guid.Empty.ToString()))).Value;


            if (_isEditer)
            {

                WebMeta form = request.SendValues ?? request.Arguments;

                this.AsyncDialog("Model", anycId =>
                {
                    if (form.ContainsKey("limit") == false)
                    {

                        this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta().Put("Id", designId))
                                .RefreshEvent("Design", "System.Picture")
                                .Builder(), true);
                    }
                    PageItem[] headers = DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, Guid.Empty, Guid.Empty).OrderBy(r => r.Seq ?? 0).ToArray();//  entity.Query();

                    UISection section = UISection.Create(new UITitle("UMC"));

                    int limit = UMC.Data.Utility.Parse(form.Get("limit"), 10);
                    int start = UMC.Data.Utility.Parse(form.Get("start"), 0);

                    switch (headers.Length)
                    {
                        case 0:
                            break;
                        case 1:
                            section.Title.Title = (headers[0].ItemName);

                            break;
                        default:
                            if (start == 0)
                            {

                                List<WebMeta> items = new List<WebMeta>();
                                foreach (PageItem item in headers)
                                {

                                    items.Add(new UMC.Web.WebMeta().Put("text", item.ItemName).Put("search", new WebMeta().Put("Id", item.Id)));
                                }
                                section.Add(UICell.Create("TabFixed", new UMC.Web.WebMeta().Put("items", items)));

                            }
                            break;
                    }


                    if (designId == Guid.Empty)
                    {
                        switch (headers.Length)
                        {
                            case 0:
                                break;
                            default:
                                new UIDesigner(true).Section(section, this.Context.AppKey ?? Guid.Empty, headers[0].Id.Value);
                                break;
                        }
                    }
                    else
                    {
                        new UIDesigner(true).Section(section, this.Context.AppKey ?? Guid.Empty, designId);
                    }
                    if (section.Length == 0)
                    {

                        section.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有设计分类项，请添加").Put("icon", "\uEA05")
                            , new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                                new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));
                    }
                    UIFootBar footer = new UIFootBar();
                    footer.IsFixed = true;// e);

                    switch (headers.Length)
                    {
                        case 0:

                            footer.AddText(new UIEventText("添加分类项").Click(new UIClick("Model", "News", "Type", "Append").Send(request.Model, request.Command)));
                            break;
                        default:
                            Guid did = designId;
                            if (designId == Guid.Empty)
                            {

                                did = headers[0].Id.Value;

                            }
                            footer.AddIcon(new UIEventText("分类项").Icon('\uf009').Click(new UIClick("Model", "News", "Id", did.ToString()).Send(request.Model, request.Command)));


                            footer.AddText(new UIEventText("增加UI组件").Click(new UIClick(did.ToString()).Send("Design", "Item")));
                            footer.AddText(new UIEventText("查看效果").Style(new UIStyle().BgColor())
                                    .Click(new UIClick("Model", "News", "Type", "View").Send(request.Model, request.Command)));


                            break;
                    }


                    section.UIFootBar = (footer);
                    response.Redirect(section);
                    return this.DialogValue("none");
                });
                String type = this.AsyncDialog("Type", g =>
                {
                    UIRadioDialog di = new UIRadioDialog();
                    di.Title = ("页面设计");
                    di.Options.Put("编辑分类项", "Edit");
                    di.Options.Put("增加分类项", "Append");
                    di.Options.Put("删除此分类", "Delete");
                    return di;
                });
                switch (type)
                {
                    case "Edit":
                        Design(designId);
                        break;
                    case "Append":
                        Design(Guid.NewGuid());
                        break;
                    case "Delete":
                        Delete(designId);
                        break;
                    case "View":
                        if (request.IsApp)
                        {
                            List<WebMeta> tabs = new List<WebMeta>();


                            UMC.Data.Utility.Each(DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, Guid.Empty, Guid.Empty).OrderBy(r => r.Seq ?? 0), dr =>
                            {

                                tabs.Add(new UMC.Web.WebMeta().Put("text", dr.ItemName).Put("search", new UMC.Web.WebMeta().Put("Id", dr.Id.ToString())).Put("cmd", "Home", "model", "UI"));

                            });
                            if (tabs.Count == 1)
                            {
                                UISectionBuilder builder = new UISectionBuilder("UI", "Home", new WebMeta().Put("Id", tabs[0].GetMeta("search").Get("Id")));
                                this.Context.Send(builder.Builder(), true);


                            }
                            else
                            {

                                this.Context.Send("Tab", new WebMeta().Put("sections", tabs).Put("text", "UMC界面设计"), true);

                            }
                        }
                        else
                        {

                            this.AsyncDialog("From", k =>
                            {

                                UIFormDialog fm = new UMC.Web.UIFormDialog();
                                fm.Title = ("移动效果体验");
                                var url = new Uri(request.Url, "/UMC/UI/Home/");
                                fm.AddImage(new Uri(UMC.Data.Utility.QRUrl(url.AbsoluteUri)));


                                fm.AddPrompt("请用支持UMC协议的APP“扫一扫”。");
                                fm.HideSubmit();

                                return fm;
                            });
                            break;
                        }
                        break;
                }

            }
            else
            {


                if (designId == Guid.Empty)
                {
                    var Name = this.AsyncDialog("Name", "none");
                    switch (Name)
                    {
                        case "none":

                            break;
                        default:
                            var item = DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, Guid.Empty, Guid.Empty).FirstOrDefault(r => String.Equals(r.ItemName, Name, StringComparison.CurrentCultureIgnoreCase)) ?? new PageItem() { Id = Utility.Guid(Name, true), ItemName = Name };


                            UIDesigner designer = new UIDesigner(false);
                            response.Redirect(designer.Section(item.ItemName, this.Context.AppKey ?? Guid.Empty, item.Id.Value));
                            break;
                    }

                    List<WebMeta> tabs = new List<WebMeta>();

                    UMC.Data.Utility.Each(DataFactory.Instance().DesignItems(this.Context.AppKey ?? Guid.Empty, Guid.Empty, Guid.Empty).OrderBy(r => r.Seq ?? 0), dr =>
                                    {
                                        tabs.Add(new UMC.Web.WebMeta().Put("text", dr.ItemName).Put("search", new UMC.Web.WebMeta().Put("Id", dr.Id)).Put("cmd", "Home", "model", "UI"));

                                    });

                    var chash = new Hashtable();
                    UITitle title = new UITitle("UMC移动界面");
                    title.Left('\uea0e', UIClick.Search());

                    title.Right(new UIEventText().Icon('\uf2c0').Click(new UIClick().Send("Account", "Info")));


                    chash.Add("sections", tabs);
                    chash.Add("title", title);
                    response.Redirect(chash);

                }
                else
                {
                    UIDesigner designer = new UIDesigner(false);
                    response.Redirect(designer.Section("", this.Context.AppKey ?? Guid.Empty, designId));

                }
            }

        }

    }

}