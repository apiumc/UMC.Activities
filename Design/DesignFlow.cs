
using UMC.Web;

namespace UMC.Activities
{

    [Mapping("Design", Auth = WebAuthType.All, Desc = "UI设计")]
    class DesignFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "DesignKey":
                    return new DesignKeyItemActivity();
                case "WebResource":
                    return new DesignWebResourcActivity();
                case "Link":
                    return new DesignLinkActivity();
                case "Cell":
                    return new DesignCellActivity();
                case "Image":
                    return new DesignImageActivity();
                case "Picture":
                    return new DesignPictureActivity();
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
                    return new DesignUIActivity();
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
