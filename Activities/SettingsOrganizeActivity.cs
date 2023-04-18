using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web.UI;
using UMC.Web;
using UMC.Data;
using System.Linq;
using UMC.Data.Entities;

namespace UMC.Activities
{
    class SettingsOrganizeActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var sId = this.AsyncDialog("Id", dkey =>
            {

                var parentId = UMC.Data.Utility.IntParse(this.AsyncDialog("ParentId", "0"), 0);

                var form = request.SendValues ?? request.Arguments;
                var limit = form["limit"] ?? "none";
                switch (limit)
                {
                    case "PC":
                        var Type = this.AsyncDialog("Type", k => this.DialogValue("Organize"));
                        switch (Type)
                        {
                            case "User":
                                response.Redirect(UMC.Data.DataFactory.Instance().Users(new UMC.Data.Entities.Organize { Id = parentId }));
                                break;
                            default:
                                response.Redirect(UMC.Data.DataFactory.Instance().Organizes(parentId));
                                break;

                        }
                        break;
                    case "none":

                        this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                            .RefreshEvent("Settings.Organize")
                                .Builder(), true);
                        break;
                    default:

                        UISection ui;

                        UISection ui2;
                        if (parentId == 0)
                        {
                            var uTitle = new UITitle("人员组织");
                            ui = UISection.Create(uTitle);

                            //ui.AddCell('\uf16b', "新建组织", "", new UIClick("Id", "Create", "ParentId", "0").Send(request.Model, request.Command));

                            ui2 = ui;// ui.NewSection();
                        }
                        else
                        {

                            var dep = UMC.Data.DataFactory.Instance().Organize(parentId);

                            var uTitle = new UITitle(dep.Caption);
                            ui = UISection.Create(uTitle);
                            ui.AddCell('\uf112', "返回上级组织", "", new UIClick("ParentId", dep.ParentId.ToString()) { Key = "Query" });

                            ui2 = ui.NewSection();


                        }

                        var Keyword = (form["Keyword"] as string ?? String.Empty);
                        if (String.IsNullOrEmpty(Keyword))
                        {

                            var deps = UMC.Data.DataFactory.Instance().Organizes(parentId);

                            foreach (var dr in deps)
                            {
                                var cell = new UI(dr.Caption).Icon("\uf0e8");
                                cell.Click(new UIClick(new WebMeta().Put("ParentId", dr.Id)) { Key = "Query" });

                                cell.Style.Name("text").Click(new UIClick(dkey, dr.Id.ToString()).Send(request.Model, request.Command)).Color(0x111);
                                ui2.Add(cell);
                            }

                            UMC.Data.Utility.Each(UMC.Data.DataFactory.Instance().Users(new Organize { Id = parentId }), dr =>
                            {
                                ui2.AddCell('\uf007', dr.Alias, dr.Username, new UIClick(dr.Id.ToString()).Send(request.Model, "User"));
                            });
                            if (parentId == 0 && ui2.Length == 0)
                            {
                                ui2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有组织，请新建").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                           new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                            }
                        }
                        else
                        {
                            int total = 0;

                            var users = Data.DataFactory.Instance().Search(new User { Username = Keyword, Alias = Keyword }, 0, 50, out total);

                            UMC.Data.Utility.Each(users, dr =>
                            {
                                ui2.AddCell('\uf007', dr.Alias, dr.Username, new UIClick(dr.Id.ToString()).Send(request.Model, "User"));
                            });
                            if (ui2.Length == 0)
                            {
                                ui2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有搜索到对应账号").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                               new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));
                            }
                          
                        }
                        ui.UIFootBar = new UIFootBar() { IsFixed = true };
                        ui.UIFootBar.AddText(new UIEventText("新建账户").Click(new UIClick("Id", "Create", "OrganizeId", parentId.ToString()).Send(request.Model, "User")),
                         new UIEventText("新建组织").Click(new UIClick("Id", "Create", "ParentId", parentId.ToString()).Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));


                        response.Redirect(ui);
                        break;
                }


                return this.DialogValue("0");
            });

            if (sId.IndexOf(',') > 0)
            {
                var sids = sId.Split(',');
                for (var i = 0; i < sids.Length; i++)
                {
                    UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Organize { Id = UMC.Data.Utility.IntParse(sids[i], 0), ModifyTime = DateTime.Now, Seq = i });
                }
                this.Context.End();
            }

            var Id = UMC.Data.Utility.IntParse(sId, 0);

            var organize = UMC.Data.DataFactory.Instance().Organize(Id) ?? new UMC.Data.Entities.Organize();




            var userValue = this.AsyncDialog("Settings", d =>
            {
                var fdlg = new Web.UIFormDialog();


                if (organize.Id.HasValue)
                {
                    var org = UMC.Data.DataFactory.Instance().Organize(organize.ParentId.Value) ?? new UMC.Data.Entities.Organize()
                    {
                        Id = 0,
                        Caption = "领层"
                    };

                    fdlg.AddTextValue().Put("上级组织", org.Caption);// "领层");
                    //fdlg.AddOption("上级组织", "ParentId", org.Id.ToString(), org.Caption)

                    //.Command("Settings", "SelectOrganize", new WebMeta().Put("Key", "ParentId").Put("NDep", organize.Id));

                    fdlg.AddText("组织名称", "Caption", organize.Caption);
                    fdlg.Title = "编辑组织";
                    if (UMC.Data.DataFactory.Instance().Organizes(organize.Id.Value).Length == 0)
                    {
                        fdlg.AddCheckBox("", "Status", "n").Add("删除此组织", "Del");
                    }
                    fdlg.Submit("确认", "Settings.Organize");
                }
                else
                {
                    fdlg.Title = "新建组织";
                    var arg = request.SendValues ?? request.Arguments;
                    var parentId = UMC.Data.Utility.IntParse(arg["ParentId"], 0);// ?? Guid.Empty;


                    if (parentId == 0)
                    {
                        fdlg.AddTextValue().Put("上级组织", "领层");
                        request.Arguments.Put("ParentId", parentId);
                    }
                    else
                    {

                        var org = UMC.Data.DataFactory.Instance().Organize(parentId);

                        if (org.ParentId == 0)
                        {
                            fdlg.AddSelect("上级组织", "ParentId")
                            .Put(org.Caption, org.Id.ToString(), true).Put("领层", Guid.Empty.ToString());
                        }
                        else
                        {

                            request.Arguments.Put("ParentId", parentId);
                            fdlg.AddTextValue().Put("上级组织", org.Caption);

                        }

                    }
                    fdlg.AddText("组织名称", "Caption", organize.Caption);

                    fdlg.Submit("确认", "Settings.Organize");
                }




                return fdlg;
            });
            var nore = new UMC.Data.Entities.Organize { Id = organize.Id };
            UMC.Data.Reflection.SetProperty(nore, userValue.GetDictionary());
            if (organize.Id.HasValue)
            {

                if ((userValue["Status"] ?? "").Contains("Del"))
                {
                    if (UMC.Data.DataFactory.Instance().Users(new UMC.Data.Entities.Organize { Id = organize.Id.Value }).Length > 0)
                    {
                        this.Prompt("此组织拥有成员");
                    }
                    else if (UMC.Data.DataFactory.Instance().Organizes(organize.Id.Value).Length > 0)
                    {
                        this.Prompt("此组织拥有下级组织");
                    }
                    else
                    {
                        UMC.Data.DataFactory.Instance().Delete(organize);
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Parent", true), true);
                    }

                }
                else
                {
                    nore.ModifyTime = DateTime.Now;

                    UMC.Data.DataFactory.Instance().Put(nore);
                    this.Prompt("更新成功", false);
                    if (nore.ParentId == organize.ParentId)
                    {
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Caption", organize.Caption), true);

                    }
                    else
                    {
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Top", true), true);

                    }

                }
            }
            else
            {
                if (nore.ParentId.HasValue == false)
                {
                    nore.ParentId = 0;
                }
                nore.Id = UMC.Data.Utility.TimeSpan();

                nore.ModifyTime = DateTime.Now;
                nore.Seq = UMC.Data.Utility.TimeSpan();
                UMC.Data.DataFactory.Instance().Put(nore);
                this.Prompt("添加成功", false);
                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Top", nore.ParentId == 0), true);

            }

        }

    }
}