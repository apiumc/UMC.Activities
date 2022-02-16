using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web;
using UMC.Data;
using System.Collections;
using UMC.Web.UI;
namespace UMC.Activities
{
    class UISettingsActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;


            var header = new UIHeader();
            var title = UITitle.Create();
            title.Title = "设置";
            var ui = UISection.Create(title);
            if (user.IsAuthenticated)
            {
                ui.NewSection().AddCell("我的账户", new Web.UIClick() { Model = "Account", Command = "Self" });
            }
            else
            {
                ui.NewSection().AddCell("未登录", "请登录", new Web.UIClick() { Model = "Account", Command = "Login" });

            }
            ui.NewSection()
                .AddCell("清空缓存", "", new Web.UIClick() { Key = "ClearCache" })
                .AddCell("检查更新", "", new Web.UIClick("Version") { Model = "UI", Command = "App" })
                .AddCell("应用测试", "", new Web.UIClick() { Model = "UI", Command = "Demo" })
                .AddCell("关于作者", "", Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "365lu/help/AboutUs"), true));



            if (user.IsAuthenticated)
            {
                var cell = UICell.Create("UI", new UMC.Web.WebMeta().Put("text", "退出登录").Put("Icon", '\uf011').Put("click", new UIClick()
                {
                    Model = "Account",
                    Command = "Close"
                }));
                cell.Style.Name("text", new UIStyle().Color(0xf00));
                ui.NewSection().NewSection()
                    .Add(cell);

            }
            response.Redirect(ui);


        }

    }

}
