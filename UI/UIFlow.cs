using System;
using System.Collections.Generic;
using System.Text;
using UMC.Web;

namespace UMC.Activities
{
    [Mapping("UI", Auth = WebAuthType.All, Desc = "UI页面")]
    public class UIFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "Demo":
                    return new UIDemoActivity();
                case "App":
                    return new UIAppActivity();
                case "Home":
                    return new DesignPageActivity(false);
                case "Search":
                    return new UISearchActivity();
                case "Config":
                    return new UIConfigActivity();
                case "Setting":
                    return new UISettingsActivity();
                default:
                    return WebActivity.Empty;

            }

        }

    }
}
