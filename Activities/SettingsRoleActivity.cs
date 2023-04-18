

using System;
using UMC.Web;

namespace UMC.Activities
{
    public class SettingsRoleActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var site = UMC.Data.Utility.IntParse(this.AsyncDialog("Site", "0"), 0);
            var strUser = this.AsyncDialog("Id", uk =>
            {
                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                        .RefreshEvent($"{request.Model}.{request.Command}")
                            .Builder(), true);
                }

                var uTitle = new UITitle("账户角色");
                var ui = UISection.Create(uTitle);
                uTitle.Right(new UIEventText("新建").Click(new UIClick(new WebMeta(request.Arguments).Put(uk, "News")).Send(request.Model, request.Command)));

                var roles = UMC.Data.DataFactory.Instance().Roles(site);

                ui.NewSection().AddCell(UMC.Security.Membership.AdminRole, "管理员组")
                       .AddCell(UMC.Security.Membership.UserRole, "用户组");



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
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理账户角色");
            }
            switch (strUser)
            {
                case UMC.Security.Membership.AdminRole:
                case UMC.Security.Membership.UserRole:
                case UMC.Security.Membership.GuestRole:
                    this.Prompt("基础角色不用再次维护");
                    break;
            }

            var role = Data.DataFactory.Instance().Role(site, strUser) ?? new Data.Entities.Role();

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
        }
    }


}