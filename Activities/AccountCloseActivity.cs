


using UMC.Web;
namespace UMC.Activities
{

    class AccountCloseActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            this.AsyncDialog("Confirm", g => new UIConfirmDialog("确认退出吗"));
            UMC.Security.AccessToken.SignOut();


            this.Prompt("退出成功", false);
            this.Context.Send("Close", false);

        }
    }
}