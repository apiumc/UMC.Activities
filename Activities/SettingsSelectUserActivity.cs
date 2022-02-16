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
    /// 查找用户
    /// </summary>
    [Mapping("Settings", "SelectUser", Auth = WebAuthType.User, Desc = "查找用户", Category = 1)]
    public class SettingsSelectUserActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var key = this.AsyncDialog("Key", g => this.DialogValue(request.Command));

            var DepId = Utility.Guid(this.AsyncDialog("Dep", g => this.DialogValue(Guid.Empty.ToString()))) ?? Guid.Empty;

            var userId = this.AsyncDialog("UserId", g =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new UMC.Web.WebMeta().Put("Key", key).Put("Dep", DepId))
                        .CloseEvent("UI.Event")
                            .Builder(), true);
                }
                UISection ui2, ui;

                if (DepId == Guid.Empty)
                {
                    var uTitle = new UITitle("选择联系人");
                    ui2 = ui = UISection.Create(new UIHeader().Search("搜索"), uTitle);
                }
                else
                {
                    var dep = UMC.Data.DataFactory.Instance().Organize(DepId);
                    var uTitle = new UITitle(dep.Caption);
                    ui2 = UISection.Create(new UIHeader().Search("搜索"), uTitle);

                    ui2.AddCell('\uf112', "返回上级部门", "", new UIClick("Dep", dep.ParentId.ToString()) { Key = "Query" });
                    ui = ui2.NewSection();
                }

                var form = request.SendValues ?? new UMC.Web.WebMeta();
                int start = UMC.Data.Utility.IntParse(form["start"], 0);
                var Keyword = (form["Keyword"] as string ?? String.Empty);

                if (String.IsNullOrEmpty(Keyword))
                {

                    UMC.Data.Utility.Each(UMC.Data.DataFactory.Instance().Organizes(DepId), dr =>
                     {
                         ui.AddCell('\uf0e8', dr.Caption, "", new UIClick(new WebMeta().Put("Dep", dr.Id)) { Key = "Query" });

                     });

                    if (DepId != Guid.Empty)
                    {
                        var uui = ui.Length == 0 ? ui : ui.NewSection();

                        UMC.Data.Utility.Each(UMC.Data.DataFactory.Instance().Users(new Organize { Id = DepId }), dr =>
                         {
                             uui.AddCell('\uf007', String.Format("{0}({1})", dr.Alias, dr.Username), String.Empty, new UIClick(new WebMeta().Put("Key", key).Put("UserId", dr.Username)).Send(request.Model, request.Command));
                         });

                    }
                }
                else
                {
                    var uui = ui;

                    int total = 0;
                    //var orgIds = new List<Guid>();
                    //var orgs = new List<Organize>();

                    var users = Data.DataFactory.Instance().Search(new User { Username = Keyword, Alias = Keyword }, out total, 0, 50);
                    //UMC.Data.Utility.Each(users, dr =>
                    //{
                    //    if (dr.OrganizeId.HasValue && orgIds.Exists(r => r == dr.OrganizeId) == false)
                    //    {
                    //        orgIds.Add(dr.Id.Value);
                    //    }

                    //});
                    //if (orgIds.Count > 0)
                    //{
                    //    orgs.AddRange(Data.DataFactory.Instance().Organize(orgIds.ToArray()));
                    //}
                    UMC.Data.Utility.Each(users, dr =>
                    {
                        //var org = orgs.Find(o => o.Id == dr.OrganizeId);
                        uui.AddCell('\uf007', dr.Alias, dr.Username, new UIClick(new WebMeta().Put("Key", key).Put("UserId", dr.Username)).Send(request.Model, request.Command));
                    });
                    if (uui.Length == 0)
                    {

                        uui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有搜索对应账号").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                       new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));
                    }
                }
                response.Redirect(ui2);
                return this.DialogValue("none");


            });
            var user = Data.DataFactory.Instance().User(userId);

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new ListItem() { Value = user.Username, Text = user.Alias }), true);
        }

    }
}
