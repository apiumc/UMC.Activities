
using System;
using System.Collections;
using UMC.Activities.Entities;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{ 
    class DesignLinkActivity : DesignClickActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Key = this.AsyncDialog("Key", "none");
            var UI = this.AsyncDialog("UI", "none");
            var click = this.Click(new UIClick());
            this.Context.Send(new UMC.Web.WebMeta().Put("type", "Click"), false);
            this.Context.Send(new UMC.Web.WebMeta().UIEvent(Key, UI, click), true);
        }
    }
}