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
    /// 
    /// </summary>
    [Mapping("Settings", "SelectRole", Auth = WebAuthType.User, Desc = "查找角色", Category = 1)]
    public class SettingsSelectRoleActivity : WebActivity
    {
        ListItem User()
        {

            var Username = this.AsyncDialog("Role", g => new RoleDialog()
            {
                CloseEvent = "UI.Event",
                IsPage = true,
                Title = "选择角色",
                IsSearch = true
            });


            var role = UMC.Data.DataFactory.Instance().Role(Username);
            return new ListItem(role.Rolename, role.Rolename);
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var key = this.AsyncDialog("Key", g => this.DialogValue("Promotion"));

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, this.User()), true);
        }

    }
}
