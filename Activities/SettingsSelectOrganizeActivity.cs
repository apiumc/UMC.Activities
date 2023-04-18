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
            var ParentId = Utility.IntParse(this.AsyncDialog("ParentId", g =>
            {
                var organizes = UMC.Data.DataFactory.Instance().Organizes(new User
                {
                    Id = this.Context.Token.UserId.Value
                });

                if (organizes.Length > 0)
                {
                    return this.DialogValue(organizes[0].Id.ToString());
                }
                return this.DialogValue("0");
            }), 0);

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
                if (ParentId == 0)
                {
                    var uTitle = new UITitle("选择组织");
                    ui = UISection.Create(uTitle);
                    ui2 = ui;
                }
                else
                {
                    var dep = UMC.Data.DataFactory.Instance().Organize(ParentId);

                    var uTitle = new UITitle(dep.Caption);
                    ui = UISection.Create(uTitle);
                    ui.AddCell('\uf112', "返回上级组织", "", new UIClick("ParentId", dep.ParentId.ToString()) { Key = "Query" });

                    ui2 = ui.NewSection();


                }

                var form = request.SendValues ?? new UMC.Web.WebMeta();
                int start = UMC.Data.Utility.IntParse(form["start"], 0);
                var Keyword = (form["Keyword"] as string ?? String.Empty);

                var deps = UMC.Data.DataFactory.Instance().Organizes(ParentId);



                foreach (var dr in deps)
                {

                    var data = new WebMeta().Put("text", dr.Caption).Put("Icon", "\uf0e8");
                    var cell = UICell.Create("UI", data);
                    ui2.Add(cell);

                    data.Put("click", new UIClick(new WebMeta().Put("ParentId", dr.Id)) { Key = "Query" });
                    cell.Style.Name("text").Click(new UIClick(new WebMeta().Put("Key", key).Put("Organize", dr.Id).Put("Dep", "1")).Send(request.Model, request.Command)).Color(0x111);


                }
                if (deps.Length == 0)
                {
                    if (ParentId == 0)
                    {
                        ui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有组织架构").Put("icon", "\uf0e8"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                  new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                    }
                    else
                    {
                        return this.DialogValue(ParentId.ToString());
                    }
                }

                response.Redirect(ui);
                return this.DialogValue("none");


            });
            ;
            var org = UMC.Data.DataFactory.Instance().Organize(UMC.Data.Utility.IntParse(OrganizeId, 0));

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new ListItem()
            {
                Value = org.Id.ToString(),
                Text = org.Caption
            }), true);

        }
    }
}
