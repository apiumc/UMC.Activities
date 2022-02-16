using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web;

namespace UMC.Activities
{
    [Mapping("Account", Desc = "客户服务")]
    class AccountFlow : WebFlow
    {

        public override Web.WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "Menu":
                    return new AccountMenuActivity();
                case "Check":
                    return new AccountCheckActivity();
                case "Login":
                    return new AccountLoginActivity();
                case "Register":
                    return new AccountRegisterActivity();
                case "Forget":
                    return new AccountForgetActivity();
                case "Password":
                    return new AccountPasswordActivity();
                case "Self":
                    return new AccountSelfActivity();
                case "Email":
                    return new AccountEmailActivity();
                case "Mobile":
                    return new AccountMobileActivity();
                case "Close":
                    return new AccountCloseActivity();
            }
            return Web.WebActivity.Empty;
        }

        public override Web.WebActivity GetNextActivity(string ActivityHeader)
        {
            return WebActivity.Empty;
        }
    }
}