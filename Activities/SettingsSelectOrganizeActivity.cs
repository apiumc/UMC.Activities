using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    /// <summary>
    /// 选择组织架构
    /// </summary>
    [Mapping("Settings", "SelectOrganize", Auth = WebAuthType.User, Desc = "选择组织架构", Category = 1)]
    public class SettingsSelectOrganizeActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var key = this.AsyncDialog("Key", g => this.DialogValue("Organize"));
            var DepId = Utility.Guid(this.AsyncDialog("Dep", g =>
            {
                var organizes = UMC.Data.DataFactory.Instance().Organizes(new User { Id = this.Context.Token.Id.Value });


                if (organizes.Length > 0)
                {
                    return this.DialogValue(organizes[0].Id.ToString());// dep.ID

                }
                return this.DialogValue(Guid.Empty.ToString());
            })) ?? Guid.Empty;



            var NDepId = Utility.Guid(this.AsyncDialog("NDep", g =>
            {
                return this.DialogValue(Guid.Empty.ToString());
            })) ?? Guid.Empty;



            var OrganizeId = this.AsyncDialog("Organize", g =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                        .CloseEvent("UI.Event")
                            .Builder(), true);
                }
                UISection ui;

                UISection ui2;
                if (DepId == Guid.Empty)
                {
                    var uTitle = new UITitle("选择部门");
                    ui = UISection.Create(uTitle);
                    ui2 = ui;
                }
                else
                {
                    var dep = UMC.Data.DataFactory.Instance().Organize(DepId);

                    var uTitle = new UITitle(dep.Caption);
                    ui = UISection.Create(uTitle);
                    ui.AddCell('\uf112', "返回上级部门", "", new UIClick("Dep", dep.ParentId.ToString()) { Key = "Query" });

                    ui2 = ui.NewSection();


                }

                var form = request.SendValues ?? new UMC.Web.WebMeta();
                int start = UMC.Data.Utility.IntParse(form["start"], 0);
                var Keyword = (form["Keyword"] as string ?? String.Empty);

                var deps = UMC.Data.DataFactory.Instance().Organizes(DepId);// new List<ORGDEPARTMENT>();


                int ct = 0;

                foreach (var dr in deps)
                {
                    if (dr.Id.Value != NDepId)
                    {
                        ct++;
                        var data = new WebMeta().Put("text", dr.Caption).Put("Icon", "\uf0e8");//.Put("Sel", "选中").Put("Next", "选中此部门");
                        var cell = UICell.Create("UI", data);
                        ui2.Add(cell);

                        data.Put("click", new UIClick(new WebMeta().Put("Dep", dr.Id)) { Key = "Query" });
                        cell.Style.Name("text").Click(new UIClick(new WebMeta().Put("Key", key).Put("Organize", dr.Id).Put("Dep", "1")).Send(request.Model, request.Command)).Color(0x111);
                    }

                }
                if (ct == 0)
                {
                    if (DepId == Guid.Empty)
                    {
                        ui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有组织架构").Put("icon", "\uf0e8"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                  new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                    }
                    else
                    {
                        return this.DialogValue(DepId.ToString());
                    }
                }

                response.Redirect(ui);
                return this.DialogValue("none");


            });
            ;
            var org = UMC.Data.DataFactory.Instance().Organize(new Guid(OrganizeId));

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new ListItem()
            {
                Value = org.Id.ToString(),
                Text = org.Caption
            }), true);

        }
    }
}
