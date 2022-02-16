
using UMC.Web;

namespace UMC.Activities
{

    [Mapping("Settings", Auth = WebAuthType.Admin, Desc = "后台设置服务")]
    public class SettingsFlow : WebFlow
    {

        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "Organize":
                    return new SettingsOrganizeActivity();
                case "AuthKey":
                    return new SettingsAuthKeyActivity();
                case "Menu":
                    return new SettingsMenuActivity();
                case "Auth":
                    return new SettingsAuthActivity();
                case "Role":
                    return new SettingsRoleActivity();
                case "User":
                    return new SettingsUserActivity();

            }

            return WebActivity.Empty;
        }
    }
}
