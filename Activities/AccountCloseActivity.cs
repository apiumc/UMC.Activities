


using UMC.Web;
namespace UMC.Activities
{

    class AccountCloseActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            this.AsyncDialog("Confirm", g => new UIConfirmDialog("确认退出吗"));
            this.Context.Token.SignOut().Commit(request.UserHostAddress, this.Context.Server);


            this.Prompt("退出成功", false);
            this.Context.Send("Close", false);

        }
    }
}