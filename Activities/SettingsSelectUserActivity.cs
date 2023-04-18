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

            var organizeId = Utility.IntParse(this.AsyncDialog("Organize", "0"), 0);// ?? Guid.Empty;

            var userId = this.AsyncDialog("Username", g =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command,  request.Arguments)
                        .CloseEvent("UI.Event")
                            .Builder(), true);
                }
                UISection ui2, ui;

                if (organizeId == 0)
                {
                    var uTitle = new UITitle("选择联系人");
                    ui2 = ui = UISection.Create(new UIHeader().Search("搜索"), uTitle);
                }
                else
                {
                    var dep = UMC.Data.DataFactory.Instance().Organize(organizeId);
                    var uTitle = new UITitle(dep.Caption);
                    ui2 = UISection.Create(new UIHeader().Search("搜索"), uTitle);

                    ui2.AddCell('\uf112', "返回上级部门", "", new UIClick("Organize", dep.ParentId.ToString()) { Key = "Query" });
                    ui = ui2.NewSection();
                }

                var form = request.SendValues ?? new UMC.Web.WebMeta();
                int start = UMC.Data.Utility.IntParse(form["start"], 0);
                var Keyword = (form["Keyword"] as string ?? String.Empty);

                if (String.IsNullOrEmpty(Keyword))
                {

                    UMC.Data.Utility.Each(UMC.Data.DataFactory.Instance().Organizes(organizeId), dr =>
                     {
                         ui.AddCell('\uf0e8', dr.Caption, "", new UIClick(new WebMeta().Put("Organize", dr.Id)) { Key = "Query" });

                     });

                    if (organizeId != 0)
                    {
                        var uui = ui.Length == 0 ? ui : ui.NewSection();

                        UMC.Data.Utility.Each(UMC.Data.DataFactory.Instance().Users(new Organize { Id = organizeId }), dr =>
                         {
                             uui.AddCell('\uf007', String.Format("{0}({1})", dr.Alias, dr.Username), String.Empty, new UIClick(new WebMeta().Put("Key", key).Put("Username", dr.Username)).Send(request.Model, request.Command));
                         });

                    }
                }
                else
                {
                    var uui = ui;

                    int total = 0; 

                    var users = Data.DataFactory.Instance().Search(new User { Username = Keyword, Alias = Keyword }, 0, 50, out total);
                     
                    UMC.Data.Utility.Each(users, dr =>
                    {
                        uui.AddCell('\uf007', dr.Alias, dr.Username, new UIClick(new WebMeta().Put("Key", key).Put("Username", dr.Username)).Send(request.Model, request.Command));
                    });
                    if (uui.Length == 0)
                    {

                        uui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有搜索到对应账号").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
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
