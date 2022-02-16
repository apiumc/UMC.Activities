using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Web;
using UMC.Data;
using UMC.Security;

namespace UMC.Activities
{
    class SettingsAuthKeyActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var name = this.AsyncDialog("Key", r => new UITextDialog() { Title = "通配符" });
            var wdk = UMC.Data.DataFactory.Instance().Wildcard(name);

            var auths = new List<UMC.Security.Authorize>();
            if (wdk != null)
            {
                var data = new Data.Entity<Data.Entities.Wildcard, List<Security.Authorize>>(wdk, wdk.Authorizes);

                auths.AddRange(data.Config);
            }
            var Type = this.AsyncDialog("Type", gg =>
            {
                var form = request.SendValues ?? new UMC.Web.WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta(request.Arguments.GetDictionary()))
                        .RefreshEvent("Wildcard")
                            .Builder(), true);
                }
                var ui = UMC.Web.UISection.Create(new UITitle("权限设置"));
                ui.AddCell('\uf084', "标识", name);


                var ui3 = ui.NewSection().AddCell('\uf007', "许可用户", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "User")).Send(request.Model, request.Command));
                var users = auths.FindAll(g => g.Type == Security.AuthorizeType.UserAllow);
                var uids = new List<String>();
                foreach (var u in users)
                {
                    uids.Add(u.Value);
                }

                var dusers = UMC.Security.Membership.Instance().Identity(uids.ToArray());


                foreach (var u in users)
                {
                    var text = u.Value;
                    var u1 = dusers.Find(d => d.Name == u.Value);
                    if (u1 != null)
                    {
                        text = u1.Alias;
                    }
                    var cell = UICell.Create("Cell", new WebMeta().Put("value", u.Value).Put("text", text));//.Put("Icon", '\uf007'));

                    ui3.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Value)).Send(request.Model, request.Command)));
                }
                if (users.Count == 0)
                {
                    ui3.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可用户").Put("icon", "\uf007"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                 new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                var ui2 = ui.NewSection().AddCell('\uf0c0', "许可角色", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "Role")).Send(request.Model, request.Command));

                var roles = auths.FindAll(g => g.Type == Security.AuthorizeType.RoleAllow);

                foreach (var u in roles)
                {
                    var cell = UICell.Create("Cell", new WebMeta().Put("text", u.Value));//.Put("Icon", '\uf0c0'));

                    ui2.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Value)).Send(request.Model, request.Command)));
                }
                if (roles.Count == 0)
                {
                    ui2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可角色").Put("icon", "\uf0c0"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                var ui4 = ui.NewSection().AddCell('\uf0e8', "许可组织", "", new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, "Organize")).Send(request.Model, request.Command));

                var orgids = new List<Guid>();

               auths.FindAll(g =>
                {
                    if (g.Type == Security.AuthorizeType.OrganizeAllow)
                    {
                        orgids.Add(Utility.Guid(g.Value) ?? Guid.Empty);
                        return true;
                    }
                    return false;
                });
                var organizes = UMC.Data.DataFactory.Instance().Organize(orgids.ToArray());

                foreach (var u in organizes)
                {
                   // orgids.Add(Utility.Guid(u.Value) ?? Guid.Empty);
                    var cell = UICell.Create("Cell", new WebMeta().Put("text", u.Caption));//.Put("Icon", '\uf0c0'));

                    ui4.Delete(cell, new UIEventText().Click(new Web.UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(gg, u.Id)).Send(request.Model, request.Command)));
                }
                if (organizes.Length == 0)
                {
                    ui4.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未设置许可角色").Put("icon", "\uf0e8"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }

                response.Redirect(ui);
                return this.DialogValue("none");
            });
            switch (Type)
            {
                case "Role":
                    var role = this.AsyncDialog("SelectRole", request.Model, "SelectRole");
                    auths.RemoveAll(k => String.Equals(k.Value, role));
                    auths.Add(new Security.Authorize { Type = UMC.Security.AuthorizeType.RoleAllow, Value = role });

                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Wildcard { WildcardKey = name, Authorizes = UMC.Data.JSON.Serialize(auths) });
                    this.Context.Send("Wildcard", true);
                    break;
                case "User":
                    var user = this.AsyncDialog("SelectUser", request.Model, "SelectUser");
                    auths.RemoveAll(k => String.Equals(k.Value, user));
                    auths.Add(new Security.Authorize { Type = UMC.Security.AuthorizeType.UserAllow, Value = user });

                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Wildcard { WildcardKey = name, Authorizes = UMC.Data.JSON.Serialize(auths) });
                    this.Context.Send("Wildcard", true);
                    break;

                case "Organize":
                    var Organize = this.AsyncDialog("SelectOrganize", request.Model, "SelectOrganize");
                    auths.RemoveAll(k => String.Equals(k.Value, Organize));
                    auths.Add(new Security.Authorize { Type = UMC.Security.AuthorizeType.OrganizeAllow, Value = Organize });

                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Wildcard { WildcardKey = name, Authorizes = UMC.Data.JSON.Serialize(auths) });
                    this.Context.Send("Wildcard", true);
                    break;
                default:
                    var a = auths.Find(k => String.Equals(Type, k.Value));
                    if (a != null)
                    {
                        auths.Remove(a);

                        UMC.Data.DataFactory.Instance().Put(new Data.Entities.Wildcard { WildcardKey = name, Authorizes = UMC.Data.JSON.Serialize(auths) });
                        if (auths.Exists(k => k.Type == a.Type) == false)
                        {
                            this.Context.Send("Wildcard", true);

                        }
                    }
                    break;
            }
            //var acc =
        }
        public void ProcessActivity2(WebRequest request, WebResponse response)
        {

            var roles = UMC.Data.DataFactory.Instance().Roles();
            var RoleType = UMC.Web.UIDialog.AsyncDialog("Type", d =>
            {
                if (roles.Length < 4)
                {
                    return Web.UIDialog.ReturnValue("User");
                }
                var rd = new Web.UIRadioDialog() { Title = "选择设置账户类型" };
                rd.Options.Add("角色", "Role");
                rd.Options.Add("用户", "User");
                rd.Options.Add("组织", "Organize");
                return rd;
            });

            var setValue = UMC.Web.UIDialog.AsyncDialog("Value", d =>
            {
                if (RoleType == "Role")
                {
                    var rd = new Web.UIRadioDialog() { Title = "请选择设置权限的角色" };


                    Utility.Each(roles,
                        dr =>
                        {
                            switch (dr.Rolename)
                            {
                                case UMC.Security.Membership.AdminRole:
                                case UMC.Security.Membership.GuestRole:
                                    break;
                                default:
                                    rd.Options.Add(dr.Rolename, dr.Rolename);
                                    break;
                            }
                        });
                    return rd;
                }
                else
                {
                    return new UserDialog() { Title = "请选择设置权限的账户" };
                }
            });

            var wdcks = Web.WebServlet.Auths();

            var ids = new List<String>();
            Utility.Each(wdcks, g => ids.Add(g.Get("key")));
            if (wdcks.Count == 0)
            {
                this.Prompt("现在的功能不需要设置权限");
            }
            var wdks = new List<UMC.Data.Entity<UMC.Data.Entities.Wildcard, List<UMC.Security.Authorize>>>();



            Utility.Each(UMC.Data.DataFactory.Instance().Wildcard(ids.ToArray()), dr =>
            {
                wdks.Add(new Data.Entity<Data.Entities.Wildcard, List<Security.Authorize>>(dr, dr.Authorizes));
            });


            var Wildcard = Web.UIDialog.AsyncDialog("Wildcards", d =>
            {
                var fmdg = new Web.UICheckboxDialog();
                fmdg.Title = "权限设置";
                fmdg.DefaultValue = "None";


                foreach (var cm in wdcks)
                {
                    var id = cm.Get("key");

                    var wdk = wdks.Find(w => String.Equals(w.Value.WildcardKey, id, StringComparison.CurrentCultureIgnoreCase));
                    if (wdk != null)
                    {
                        if (wdk.Config != null)
                        {
                            var isS = false;
                            switch (RoleType)
                            {
                                case "Organize":

                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.OrganizeDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;
                                case "Role":
                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.RoleDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;
                                default:
                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.UserDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;

                            }
                            fmdg.Options.Add(cm.Get("desc"), id, !isS);
                        }
                        else
                        {
                            fmdg.Options.Add(cm.Get("desc"), id, true);
                        }
                    }
                    else
                    {
                        fmdg.Options.Add(cm.Get("desc"), id, true);
                    }
                }

                return fmdg;

            });
            foreach (var cm in wdcks)
            {
                var id = cm.Get("key");
                var wdk = wdks.Find(w => String.Equals(w.Value.WildcardKey, id, StringComparison.CurrentCultureIgnoreCase));

                List<Security.Authorize> authorizes;
                if (wdk != null)
                {
                    authorizes = wdk.Config;
                }
                else
                {
                    authorizes = new List<Security.Authorize>();
                }
                switch (RoleType)
                {
                    case "Role":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.RoleDeny || a.Type == Security.AuthorizeType.RoleAllow)
                      && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                    case "Organize":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.OrganizeAllow || a.Type == Security.AuthorizeType.OrganizeDeny)
                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                    case "User":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.UserAllow || a.Type == Security.AuthorizeType.UserDeny)
                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                }
                if (Wildcard.IndexOf(id) == -1)
                {
                    switch (RoleType)
                    {
                        case "Role":
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.RoleDeny });

                            break;
                        case "Organize":
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.OrganizeDeny });

                            break;
                        default:
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.UserDeny });

                            break;
                    }

                    UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Wildcard
                    {
                        Authorizes = UMC.Data.JSON.Serialize(authorizes),
                        WildcardKey = id,
                        Description = cm.Get("desc")
                    });
                }

            }
            this.Prompt("权限设置成功");

        }
    }
}