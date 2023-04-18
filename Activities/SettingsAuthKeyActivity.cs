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
    class SettingsAuthKeyActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var site = Utility.IntParse(this.AsyncDialog("Site", "0"), 0);

            var name = this.AsyncDialog("Key", r => new AuthorityDialog(site) { IsSearch = true });
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
            var wdk = UMC.Data.DataFactory.Instance().Authority(site, name);

            var auths = new List<Tuple<byte, String>>();
            if (wdk != null)
            {
                auths.AddRange(AuthManager.Authorize(wdk.Body));
            }
            var Type = this.AsyncDialog("Type", gg =>
            {
                var form = request.SendValues ?? new UMC.Web.WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta(request.Arguments.GetDictionary()))
                        .RefreshEvent("Authorize")
                            .Builder(), true);
                }
                var ui = UMC.Web.UISection.Create(new UITitle("授权管理"));
                ui.AddCell('\uf084', "标识", name, new UIClick(new WebMeta(request.Arguments).Put(gg, "Desc")).Send(request.Model, request.Command));


                var ui3 = ui.NewSection().AddCell('\uf007', "许可用户", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "User")).Send(request.Model, request.Command));
                var users = auths.FindAll(g => g.Item1 == Security.AuthorizeType.UserAllow);
                var uids = new List<String>();
                foreach (var u in users)
                {
                    uids.Add(u.Item2);
                }

                var dusers = UMC.Security.Membership.Instance().Identity(uids.ToArray());


                foreach (var u in users)
                {
                    var text = u.Item2;
                    var u1 = dusers.Find(d => d.Name == u.Item2);
                    if (u1 != null)
                    {
                        text = u1.Alias;
                    }
                    var cell = UICell.Create("Cell", new WebMeta().Put("value", u.Item2).Put("text", text));

                    ui3.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Item2)).Send(request.Model, request.Command)));
                }
                if (users.Count == 0)
                {
                    ui3.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可用户").Put("icon", "\uf007"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                 new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                var ui2 = ui.NewSection().AddCell('\uf0c0', "许可角色", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "Role")).Send(request.Model, request.Command));

                var roles = auths.FindAll(g => g.Item1 == Security.AuthorizeType.RoleAllow);

                foreach (var u in roles)
                {
                    var cell = UICell.Create("Cell", new WebMeta().Put("text", u.Item2));

                    ui2.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Item2)).Send(request.Model, request.Command)));
                }
                if (roles.Count == 0)
                {
                    ui2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可角色").Put("icon", "\uf0c0"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                var ui4 = ui.NewSection().AddCell('\uf0e8', "许可组织", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "Organize")).Send(request.Model, request.Command));

                var orgids = new List<int>();

                auths.FindAll(g =>
                 {
                     if (g.Item1 == Security.AuthorizeType.OrganizeAllow)
                     {
                         orgids.Add(Utility.IntParse(g.Item2, 0));//?? Guid.Empty);
                         return true;
                     }
                     return false;
                 });
                var organizes = UMC.Data.DataFactory.Instance().Organize(orgids.ToArray());

                foreach (var u in organizes)
                {
                    var cell = UICell.Create("Cell", new WebMeta().Put("text", u.Caption));

                    ui4.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Id)).Send(request.Model, request.Command)));
                }
                if (organizes.Length == 0)
                {
                    ui4.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可组织").Put("icon", "\uf0e8"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                response.Redirect(ui);
                return this.DialogValue("none");
            });
            switch (Type)
            {
                case "Desc":
                    var userValue = this.AsyncDialog("Settings", d =>
                    {
                        var fdlg = new Web.UIFormDialog();
                        if (String.Equals(name, "News"))
                        {
                            fdlg.Title = "新建授权路径";
                            fdlg.AddText("路径", "AuthKey", String.Empty);
                        }
                        else
                        {

                            fdlg.Title = "授权描述";
                            fdlg.AddTextValue().Put("路径", name);
                        }
                        if (wdk != null)
                        {
                            fdlg.AddText("描述", "Desc", wdk.Desc).NotRequired();
                        }
                        else
                        {
                            fdlg.AddText("描述", "Desc", String.Empty).NotRequired();

                        }
                        fdlg.Submit("确认", "Settings.Authority");
                        return fdlg;
                    });
                    if (String.Equals(name, "News"))
                    {
                        var keyAuth = userValue["AuthKey"];
                        if (keyAuth.StartsWith("/"))
                        {
                            this.Prompt("授权路径不能以“/”开头");
                        }
                        UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority
                        {
                            Key = keyAuth,
                            Desc = userValue["Desc"],
                            Site = site
                        });
                        response.Redirect(request.Model, request.Command, new WebMeta().Put("Site", site).Put("Key", keyAuth), false);
                    }
                    else
                    {
                        UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority
                        {
                            Key = name,
                            Desc = userValue["Desc"],
                            Site = site
                        });
                    }
                    this.Context.Send("Settings.Authority", true);
                    break;
                case "Del":
                    UMC.Data.DataFactory.Instance().Delete(new Data.Entities.Authority
                    {
                        Key = name,
                        Site = site
                    });
                    this.Context.Send("Settings.Authority", true);
                    break;
                case "Role":
                    var role = this.AsyncDialog("SelectRole", request.Model, "SelectRole");
                    auths.RemoveAll(k => String.Equals(k.Item2, role));
                    auths.Add(Tuple.Create(UMC.Security.AuthorizeType.RoleAllow, role));

                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority { Key = name, Site = site, Body = AuthManager.Authorize(auths.ToArray()) });
                    this.Context.Send("Authorize", true);
                    break;
                case "User":
                    var user = this.AsyncDialog("SelectUser", request.Model, "SelectUser");
                    auths.RemoveAll(k => String.Equals(k.Item2, user));
                    auths.Add(Tuple.Create(UMC.Security.AuthorizeType.UserAllow, user));

                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority { Key = name, Site = site, Body = AuthManager.Authorize(auths.ToArray()) });
                    this.Context.Send("Authorize", true);
                    break;

                case "Organize":
                    var Organize = this.AsyncDialog("SelectOrganize", request.Model, "SelectOrganize");
                    auths.RemoveAll(k => String.Equals(k.Item2, Organize));
                    auths.Add(Tuple.Create(UMC.Security.AuthorizeType.OrganizeAllow, Organize));


                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority
                    {
                        Key = name,
                        Site = site,
                        Body = AuthManager.Authorize(auths.ToArray())
                    });
                    this.Context.Send("Authorize", true);
                    break;
                default:
                    var a = auths.Find(k => String.Equals(Type, k.Item2));
                    if (a != null)
                    {
                        auths.Remove(a);

                        UMC.Data.DataFactory.Instance().Put(new Data.Entities.Authority
                        {
                            Key = name,
                            Site = site,
                            Body = AuthManager.Authorize(auths.ToArray())
                        });
                        if (auths.Exists(k => k.Item1 == a.Item1) == false)
                        {
                            this.Context.Send("Authorize", true);

                        }
                    }
                    break;
            }
        }


    }
}