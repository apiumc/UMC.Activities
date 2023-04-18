using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{

    [Mapping("Settings", "SelectRole", Auth = WebAuthType.User, Desc = "查找角色", Category = 1)]
    public class SettingsSelectRoleActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var key = this.AsyncDialog("Key", g => this.DialogValue("SelectRole"));
            var site = UMC.Data.Utility.IntParse(this.AsyncDialog("Site", "0"), 0);

            var Rolename = this.AsyncDialog("Rolename", uk =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                        .CloseEvent("UI.Event")
                        .RefreshEvent($"{request.Model}.{request.Command}")
                            .Builder(), true); ;
                }

                var uTitle = new UITitle("选择角色");
                var ui = UISection.Create(uTitle);
                if (key == "Editer")
                {
                    uTitle.Title = "角色维护";
                }
                if (request.IsMaster)
                {
                    uTitle.Right(new UIEventText("新增").Click(new UIClick("Key", "Editer", uk, "News", "Site", site.ToString()).Send(request.Model, request.Command)));

                }
                else if (site != 0)
                {
                    var rols = UMC.Data.DataFactory.Instance().Roles(this.Context.Token.UserId.Value, site);
                    if (rols.Contains(UMC.Security.Membership.AdminRole))
                    {
                        uTitle.Right(new UIEventText("新增").Click(new UIClick("Key", "Editer", uk, "News", "Site", site.ToString()).Send(request.Model, request.Command)));

                    }
                }


                ui.NewSection().AddCell(UMC.Security.Membership.AdminRole, "管理员组", new UIClick(new WebMeta(request.Arguments).Put(uk, UMC.Security.Membership.AdminRole)).Send(request.Model, request.Command))
               .AddCell(UMC.Security.Membership.UserRole, "用户组", new UIClick(new WebMeta(request.Arguments).Put(uk, UMC.Security.Membership.UserRole)).Send(request.Model, request.Command));


                var roles = UMC.Data.DataFactory.Instance().Roles(site);

                foreach (var r in roles)
                {
                    var ui2 = ui.NewSection();
                    ui2.AddCell(r.Rolename, new UIClick(new WebMeta(request.Arguments).Put(uk, r.Rolename)).Send(request.Model, request.Command));
                    if (String.IsNullOrEmpty(r.Explain))
                    {

                        ui2.Add(new UMC.Web.UI.UIDesc(r.Explain));
                    }

                }
                response.Redirect(ui);
                return this.DialogValue("none");

            });
            switch (key)
            {
                case "Editer":

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
                    switch (Rolename)
                    {
                        case UMC.Security.Membership.AdminRole:
                        case UMC.Security.Membership.UserRole:
                        case UMC.Security.Membership.GuestRole:
                            this.Prompt("基础角色不用再次维护");
                            break;
                    }
                    var role = Data.DataFactory.Instance().Role(site, Rolename) ?? new Data.Entities.Role();
                    var setting = this.AsyncDialog("Setting", d =>
                    {
                        var frm = new UIFormDialog();
                        frm.Title = "用户角色";
                        if (String.IsNullOrEmpty(role.Rolename))
                        {
                            frm.Title = "新建角色";
                            frm.AddText("角色名", "Rolename", role.Rolename);
                        }
                        else
                        {
                            frm.AddTextValue().Put("角色名", role.Rolename);
                        }
                        frm.AddTextarea("角色说明", "Explain", role.Explain).NotRequired().Put("tip", "角色说明");

                        return frm;
                    });
                    var rname = setting["Rolename"];
                    if (String.IsNullOrEmpty(rname) == false)
                    {
                        switch (rname)
                        {
                            case UMC.Security.Membership.AdminRole:
                            case UMC.Security.Membership.UserRole:
                            case UMC.Security.Membership.GuestRole:
                                this.Prompt("基础角色不用再次维护");
                                break;
                        }
                        if (Data.DataFactory.Instance().Role(site, rname) != null)
                        {
                            this.Prompt("此角色已经存在");
                        }
                        role.Rolename = rname;
                    }
                    else
                    {
                        this.Prompt("角色名不为这空");
                    }
                    role.Site = site;
                    role.Explain = setting["Explain"];
                    Data.DataFactory.Instance().Put(role);
                    this.Context.Send($"{request.Model}.{request.Command}", true);
                    break;
            }

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new ListItem(Rolename, Rolename)), true);
        }

    }
}
