
using UMC.Web;

namespace UMC.Activities
{

    [Mapping("Design", Auth = WebAuthType.All, Desc = "UI设计")]
    public class DesignFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "ItemKey":
                    return new DesignItemKeyActivity();
                case "Item":
                    return new DesignItemActivity();
                case "Click":
                    return new DesignClickActivity();
                case "Custom":
                    return new DesignCustomActivity();
                case "Items":
                    return new DesignItemsActivity();
                case "Banner":
                    return new DesignBannerActivity();
                case "Page":
                    return new DesignPageActivity();
                case "View":
                    return new DesignPageActivity(false);
                default:
                    if (this.Context.Request.Command.StartsWith("UI"))
                    {
                        return new DesignConfigActivity();
                    }
                    return WebActivity.Empty;

            }

        }
    }
}
