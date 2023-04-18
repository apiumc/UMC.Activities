using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web.UI;
using UMC.Web;
using UMC.Data;
using System.Linq;

namespace UMC.Activities
{
    class SettingsMenuActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var site = Utility.IntParse(this.AsyncDialog("Site", "0"), 0);
            if (request.IsMaster == false)
            {
                if (site != 0)
                {
                    var rols = UMC.Data.DataFactory.Instance().Roles(this.Context.Token.UserId.Value, site);
                    if (rols.Contains(UMC.Security.Membership.AdminRole) == false)
                    {
                        this.Prompt("需要管理员权限才能设置");
                    }
                }
                else
                {
                    this.Prompt("需要管理员权限才能设置");
                }
            }

            var mId = UMC.Data.Utility.IntParse(this.AsyncDialog("Id", dkey =>
            {

                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                switch (limit)
                {
                    case "PC":

                        var menus = Data.DataFactory.Instance().Menu(site).OrderBy(d => d.Seq ?? 0).ToList();
                        var menu = new List<WebMeta>();
                        foreach (var p in menus.FindAll(d => d.ParentId == 0))
                        {
                            var m = new WebMeta().Put("icon", p.Icon).Put("site", p.Site).Put("text", p.Caption).Put("id", p.Id).Put("status", p.IsHidden == true ? "隐藏" : "显示");

                            menu.Add(m);
                            var data2 = new System.Data.DataTable();
                            data2.Columns.Add("id");
                            data2.Columns.Add("site");
                            data2.Columns.Add("text");
                            data2.Columns.Add("url");
                            data2.Columns.Add("status");
                            var childs = menus.FindAll(c => c.ParentId == p.Id);
                            if (childs.Count > 0)
                            {
                                foreach (var ch in childs)
                                {
                                    data2.Rows.Add(ch.Id, ch.Site, ch.Caption, ch.Url, p.IsHidden == true ? "隐藏" : "显示");
                                }

                                m.Put("menu", data2);
                            }
                            else
                            {
                                m.Put("url", p.Url);
                            }
                        }

                        response.Redirect(menu);
                        break;
                    case "none":
                        this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                            .RefreshEvent($"{request.Model}.{request.Command}")
                                .Builder(), true);
                        break;
                    default:
                        UISection ui;

                        var parentId2 = UMC.Data.Utility.IntParse(this.AsyncDialog("ParentId", "0"), 0);
                        UISection ui2;
                        if (parentId2 == 0)
                        {
                            var uTitle = new UITitle("应用菜单");
                            uTitle.Right(new UIEventText("新增").Click(new UIClick(new WebMeta(request.Arguments).Put(dkey, 0).Put("ParentId", parentId2)).Send(request.Model, request.Command)));
                            ui = UISection.Create(uTitle);
                            ui2 = ui;
                        }
                        else
                        {
                            var dep = UMC.Data.DataFactory.Instance().Menu(site, parentId2);

                            var uTitle = new UITitle(dep.Caption);
                            ui = UISection.Create(uTitle);
                            uTitle.Right(new UIEventText("新增").Click(new UIClick(new WebMeta(request.Arguments).Put(dkey, 0).Put("ParentId", parentId2)).Send(request.Model, request.Command)));
                            ui.AddCell('\uf112', "返回上级", "", new UIClick("ParentId", dep.ParentId.ToString()) { Key = "Query" });

                            ui2 = ui.NewSection();


                        }
                        var menus2 = UMC.Data.DataFactory.Instance().Menus(site, parentId2);



                        foreach (var dr in menus2)
                        {
                            var cell = new UI(dr.Caption).Icon(dr.Icon ?? "\uf0e8").Click(new UIClick(new WebMeta().Put("ParentId", dr.Id)) { Key = "Query" });

                            cell.Style.Name("text").Click(new UIClick("Site", site.ToString(), dkey, dr.Id.ToString()).Send(request.Model, request.Command)).Color(0x111);

                            ui2.Add(cell);
                        }
                        if (menus2.Length == 0)
                        {
                            var desc = new UIDesc("未有菜单请新增");
                            desc.Put("icon", "\uf0e8").Format("desc", "{icon}\n{desc}");
                            desc.Style.Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60));
                            ui2.Add(desc);

                        }
                        response.Redirect(ui);
                        break;

                }
                return this.DialogValue("none");

            }), 0);
            var link = UMC.Data.DataFactory.Instance().Menu(site, mId) ?? new Data.Entities.Menu();
            var strP = this.AsyncDialog("ParentId", (link.ParentId ?? 0).ToString());
            switch (strP)
            {
                case "Auth":

                    if (String.IsNullOrEmpty(link.Url) == false)
                    {
                        var k = link.Url.IndexOf(':');
                        String path = link.Url;

                        var wk = new WebMeta();
                        if (k > 0)
                        {
                            var svalue = link.Url.Substring(0, k).ToLower();
                            switch (svalue)
                            {
                                case "http":
                                case "https":
                                    response.Redirect(request.Model, "AuthKey", new WebMeta().Put("Site", link.Site).Put("Key", $"Menu/{link.Id}"), true);
                                    //this.Prompt("此菜单不需要设置权限");
                                    break;
                                default:
                                    wk.Put("Site", UMC.Data.Utility.IntParse(UMC.Data.Utility.Guid(svalue, true).Value));
                                    path = link.Url.Substring(k + 1).TrimStart('/');
                                    break;
                            }

                        }
                        else
                        {
                            wk.Put("Site", link.Site ?? 0);
                        }
                        switch (path[0])
                        {
                            case '/':
                            case '#':
                                path = path.Substring(1);
                                break;
                        }
                        k = path.IndexOf('?');
                        if (k > 0)
                        {
                            path = path.Substring(0, k);
                        }
                        wk.Put("Key", path);
                        response.Redirect(request.Model, "AuthKey", wk, true);
                    }
                    else
                    {
                        this.Prompt("此菜单不需要设置权限");
                    }
                    break;
                case "Image":
                    response.Redirect("System", "Picture", new Guid(UMC.Data.Utility.MD5("Menu", link.Site, link.Id.Value)).ToString());
                    break;
            }

            var parentId = UMC.Data.Utility.IntParse(strP, 0);

            var userValue = this.AsyncDialog("Settings", d =>
            {
                var fdlg = new Web.UIFormDialog();
                fdlg.Title = "菜单设置";

                if (parentId == 0)
                {
                    fdlg.AddOption("菜单图标", "Icon", link.Icon, String.IsNullOrEmpty(link.Icon) ? "请选择" : "已选择").PlaceHolder("请参考UMC图标库")
                    .Command("System", "Icon");

                }
                else
                {
                    fdlg.AddTextValue().Put("上个菜单", Data.DataFactory.Instance().Menu(site, parentId).Caption);
                }
                fdlg.AddText("菜单标题", "Caption", link.Caption);
                if (parentId == 0)
                {
                    fdlg.AddText("菜单网址", "Url", link.Url).NotRequired();
                }
                else
                {
                    fdlg.AddText("菜单网址", "Url", link.Url);
                }
                if (link.Id.HasValue)
                {
                    fdlg.AddNumber("展示顺序", "Seq", link.Seq);
                    fdlg.AddCheckBox("", "Status", "n")
                    .Put("隐藏此菜单", "Hidden", link.IsHidden == true)

                    .Add("删除此菜单", "Del");
                }
                else
                {

                    fdlg.Title = "新建菜单";

                }


                fdlg.Submit("确认", "Settings.Menu");
                return fdlg;
            });
            UMC.Data.Reflection.SetProperty(link, userValue.GetDictionary());
            if (link.Id.HasValue)
            {
                string Status = (userValue["Status"] ?? "");
                if (Status.Contains("Del"))
                {
                    if (Data.DataFactory.Instance().Menus(site, link.Id.Value).Length > 0)
                    {
                        this.Prompt("此菜单还有子菜单");
                    }
                    else
                    {

                        Data.DataFactory.Instance().Delete(link);
                        this.Prompt("删除成功", false);
                    }
                }
                else
                {
                    link.IsHidden = (userValue["Status"] ?? "").Contains("Hidden");

                    Data.DataFactory.Instance().Put(link);
                    this.Prompt("更新成功", false);
                }
            }
            else
            {
                link.ParentId = parentId;
                link.Id = UMC.Data.Utility.TimeSpan();
                link.IsHidden = false;
                link.Site = site;
                link.Seq = UMC.Data.Utility.TimeSpan();


                Data.DataFactory.Instance().Put(link);
                this.Prompt("添加成功", false);
            }
            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Menu"), true);

        }

    }
}