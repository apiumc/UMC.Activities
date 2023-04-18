using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using UMC.Web;
using UMC.Data;
using UMC.Security;

namespace UMC.Activities
{
    class SettingsAuthActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var RoleType = this.AsyncDialog("Type", d =>
            {
                var rd = new Web.UIRadioDialog() { Title = "选择设置账户类型" };
                rd.Options.Add("角色", "Role");
                rd.Options.Add("用户", "User");
                rd.Options.Add("组织", "Organize");
                return rd;
            });
            var site = UMC.Data.Utility.IntParse(this.AsyncDialog("Site", "0"), 0);
            String setValue;
            switch (RoleType)
            {
                case "Role":
                    setValue = this.AsyncDialog("Value", request.Model, "SelectRole", new WebMeta().Put("Site", site));
                    break;
                default:
                case "User":
                    setValue = this.AsyncDialog("Value", request.Model, "SelectUser");
                    break;
                case "Organize":
                    setValue = this.AsyncDialog("Value", request.Model, "SelectOrganize");
                    break;
            }
            var wildcardKey = this.AsyncDialog("Authority", d =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                            .Builder(), true);
                }
                var ui = UISection.Create();

                var form = request.SendValues ?? new UMC.Web.WebMeta();
                int start = UMC.Data.Utility.IntParse(form["start"], 0);

                var search = new UMC.Data.Entities.Authority() { Site = site };
                var Keyword = (form["Keyword"] as string ?? String.Empty);
                if (String.IsNullOrEmpty(Keyword) == false)
                {
                    search.Key = Keyword;
                }
                else
                {

                    var nextKey = this.AsyncDialog("NextKey", g => this.DialogValue("Header"));
                    switch (nextKey)
                    {
                        case "Header":
                            ui.UIHeader = new UIHeader().Search("搜索", String.Empty, "Authority");
                            switch (RoleType)
                            {
                                case "Role":
                                    ui.Title = new UITitle("角色授权");
                                    ui.AddCell("角色名", setValue);
                                    break;
                                default:
                                case "User":
                                    ui.Title = new UITitle("账户授权");
                                    ui.AddCell("账户名", setValue);
                                    break;
                                case "Organize":
                                    ui.Title = new UITitle("组织授权");
                                    var guidId = Utility.Guid(setValue, true);
                                    var org = UMC.Data.DataFactory.Instance().Organize(Utility.IntParse(setValue, 0));
                                    if (org == null)
                                    {
                                        ui.AddCell("组织名", setValue);
                                    }
                                    else
                                    {
                                        ui.AddCell("组织名", org.Caption);
                                    }
                                    break;
                            }
                            ui = ui.NewSection();
                            break;
                    }
                }
                ui.Key = "Authority";

                int next;


                UMC.Data.Utility.Each(Data.DataFactory.Instance().Search(search, start, Utility.IntParse(limit, 25), out next), dr =>
                 {
                     var auth = AuthManager.Authorize(dr.Body);
                     var isS = false;
                     switch (RoleType)
                     {
                         case "Organize":

                             isS = auth.Exists(a => a.Item1 == Security.AuthorizeType.OrganizeDeny
                                 && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));
                             break;
                         case "Role":
                             isS = auth.Exists(a => a.Item1 == Security.AuthorizeType.RoleDeny
                                 && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));
                             break;
                         default:
                             isS = auth.Exists(a => a.Item1 == Security.AuthorizeType.UserDeny
                                 && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));
                             break;

                     }
                     var cData = new WebMeta();
                     if (String.IsNullOrEmpty(dr.Desc))
                     {

                         cData.Put("text", dr.Key);
                     }
                     else
                     {
                         cData.Put("text", dr.Desc);

                     }
                     if (isS)
                     {
                         cData.Put("Color", "#1890ff").Put("Icon", "\uea07");
                     }
                     else
                     {
                         cData.Put("Icon", "\uea01");
                     }
                     var cell = UICell.Create("UI", cData.Put("click", UIClick.Click(new UIClick(new WebMeta(request.Arguments).Put(d, dr.Key)).Send(request.Model, request.Command))));

                     ui.Add(cell);
                 });
                if (ui.Length == 0)
                {
                    if (String.IsNullOrEmpty(search.Key))
                    {
                        ui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置授权符").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
               new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                    }
                    else
                    {
                        ui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有搜索对应授权符").Put("icon", "\uEA05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
               new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                    }

                }
                ui.IsNext = next > 0;
                if (ui.IsNext.Value)
                {
                    ui.StartIndex = next;
                }
                response.Redirect(ui);

                return this.DialogValue("none");

            });
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
            var wdk = UMC.Data.DataFactory.Instance().Authority(site, wildcardKey);

            List<Tuple<byte,string>> authorizes;
            if (wdk != null)
            {
                authorizes = AuthManager.Authorize(wdk.Body);
            }
            else
            {
                authorizes = new List<Tuple<byte, string>>();
            }
            int count = 0;
            switch (RoleType)
            {
                case "Role":
                    count = authorizes.RemoveAll(a => (a.Item1  == Security.AuthorizeType.RoleDeny || a.Item1 == Security.AuthorizeType.RoleAllow)
                   && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));

                    break;
                case "Organize":
                    count = authorizes.RemoveAll(a => (a.Item1 == Security.AuthorizeType.OrganizeAllow || a.Item1 == Security.AuthorizeType.OrganizeDeny)
                    && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));

                    break;
                default:
                case "User":
                    count = authorizes.RemoveAll(a => (a.Item1 == Security.AuthorizeType.UserAllow || a.Item1 == Security.AuthorizeType.UserDeny)
                    && String.Equals(a.Item2, setValue, StringComparison.CurrentCultureIgnoreCase));

                    break;
            }
            var uiData = new WebMeta();
            if (count == 0)
            {
                switch (RoleType)
                {
                    case "Role":
                        authorizes.Add(Tuple.Create(Security.AuthorizeType.RoleAllow, setValue));

                        break;
                    case "Organize":
                        authorizes.Add(Tuple.Create(Security.AuthorizeType.OrganizeAllow, setValue));

                        break;
                    default:
                        authorizes.Add(Tuple.Create(Security.AuthorizeType.UserAllow, setValue));

                        break;
                }
                uiData.Put("Color", "#1890ff").Put("Icon", "\uea07");
            }
            else
            {

                uiData.Put("Icon", "\uea01").Put("Color", "#000");
            }
            UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Authority
            {
                Site = site,
                Body = AuthManager.Authorize(authorizes.ToArray()),
                Key = wildcardKey
            });

            var section = Utility.IntParse(this.AsyncDialog("section", g => this.DialogValue("1")), 0);
            var row = Utility.IntParse(this.AsyncDialog("row", g => this.DialogValue("1")), 0);

            new UISection.Editer(section, row).Value(uiData).Builder(this.Context, this.AsyncDialog("UI", g => this.DialogValue("none")), true);


        }
    }
}