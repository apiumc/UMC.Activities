

using System;
using UMC.Web;

namespace UMC.Activities
{
    public class SettingsRoleActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var strUser = this.AsyncDialog("Id", d =>
            {
                var dlg = new RoleDialog();
                dlg.Title = "角色管理";
                dlg.IsPage = true;
                dlg.RefreshEvent = "Role";
                if (request.IsMaster)
                {
                    dlg.Menu("新建", "Settings", "Role", new UMC.Web.WebMeta().Put("Id", "News"));
                }
                return dlg;
            });
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理账户角色");
            }

            var role = Data.DataFactory.Instance().Role(strUser) ?? new Data.Entities.Role();

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
                frm.AddTextarea("角色说明", "Explain", role.Explain).Put("tip", "角色说明");

                return frm;
            });
            var rname = setting["Rolename"];
            if (String.IsNullOrEmpty(rname) == false)
            {
                if (Data.DataFactory.Instance().Role(rname) != null)
                {
                    this.Prompt("此角色已经存在");
                }
                role.Rolename = rname;
            }
            if (String.IsNullOrEmpty(role.Rolename))
            {
                this.Prompt("角色名不为这空");
            }
            role.Explain = setting["Explain"];
            Data.DataFactory.Instance().Put(role);


            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Role"), true);
        }
    }


}