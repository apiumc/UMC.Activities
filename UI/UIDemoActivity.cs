using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using UMC.Web.UI;
using UMC.Web;
using System.Collections;
using UMC.Data;

namespace UMC.Activities
{
     class  UIDemoActivity : Web.WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            switch (request.SendValue)
            {
                case "AnimImage":

                    var uiview = new UIView();
                    uiview.Style.Width("80%");
                    uiview.Src = new Uri("http://www.365lu.cn/UserResources/1usm4ih/1599785635641/red_packet_bg.png");

                    uiview.Add("https://data.kukahome.com/css/images/logo.png", new UIStyle().Name("top", "-20%").Name("width", "25%").Name("border-radius", "50%").Name("border-width", "5%").Name("border-color", "#fff"));


                    var style = new UIStyle().Name("top", "30%").Name("width", "30%");

                    style.Name("clicked").Name("animation-name", "reverse");
                    style.Name("animation-name", "scale");

                    uiview.Add(new UIClick("Date").Send(request.Model, request.Command), new Uri("http://www.365lu.cn/UserResources/1usm4ih/1599785654433/icon_open_red_packet1.png"), style);
                  
                    uiview.Add("牛人啊，请玩红包", new UIStyle().Color(0xfff).Size(30).Name("width", "80%"));
                    this.Context.Send("UIView", new WebMeta().Put("view", uiview), true);
                    break;
                case "Date":
                    this.AsyncDialog("D", "none");
                    this.AsyncDialog("Datec", g =>
                    {
                        return UIDateDialog.CreateDialog("Time");
                    });
                    return; ;
            }
            var form = request.SendValues ?? new UMC.Web.WebMeta();
            var start = Utility.IntParse(form["start"], 0);
            var limit = Utility.IntParse(form["limit"], 1000);
            if (form.ContainsKey("limit") == false)
            {
                this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                    .RefreshEvent("Builder")
                    .Builder(), true);


            }

            var videoSrc = new Uri("http://2449.vod.myqcloud.com/2449_22ca37a6ea9011e5acaaf51d105342e3.f20.mp4");
            var ui = UISection.Create(new UITitle("Demo"));
            if (start == 0)
            {
                UIView coustomCell = new UIView("UMC_User");

                coustomCell.Style.Name("width", "50%");
                coustomCell.Src = new Uri("http://www.365lu.cn/UserResources/1usm4ih/1599785635641/red_packet_bg.png");
                coustomCell.Add("image", new Uri("http://www.365lu.cn/UserResources/1usm4ih/1599785654433/icon_open_red_packet1.png"), new UIStyle().Name("width", "20%").Name("animation-name", "reverse"));
                coustomCell.Add("Text", "你好啊，是不是很好呢", new UIStyle().Name("left", "10").AlignLeft());
                ui.Componen.Add(coustomCell);
                ui.UIHeader = new UIHeader().Coustom(coustomCell);
            }

            var footer = new UIFootBar();
            footer.AddText(new UIEventText("w磊").Style(new UIStyle().Fixed().BgColor().Name("margin", "10").Name("border-radius", "10")));
            ui.UIFootBar = footer;
            footer.IsFixed = true;
            var uIIcon = new UIIconNameDesc(new UIIconNameDesc.Item('\uF02d', "知识创作", "1篇").Color(0x36a3f7));
            ui.Add(uIIcon);
            uIIcon.Button("图片动画", new UIClick("AnimImage").Send(request.Model, request.Command), 0x36a3f7);
            uIIcon.Style.Name("fixed", "true");
            var text = new UITextDesc(new WebMeta().Put("title", "TextDesc使用说明", "desc", "格式属性title、desc、tag", "tag", "122"));
            text.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/TextDesc"), true));
            ui.Add(text);
            var cell = UICell.Create("UMC_User", new WebMeta().Put("Text", "失人啊").Put("image", "https://www.baidu.com/img/flexible/logo/pc/result.png"));
            cell.Style.Name("Text").Color(0x05d);
            ui.Add(cell);

            var imge = new UIImageTextDesc(new WebMeta().Put("title", "ImageTextDesc使用说明", "desc", "格式属性title、desc、tag", "tag", "122", "right", "right").Put("src", "https://data.kukahome.com/css/images/logo.png"));
            imge.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/ImageTextDesc"), true));
            ui.Add(imge);
            var cmT = new UICMSImage("https://data.kukahome.com/css/images/logo.png");
            cmT.Style.Name("width", "60%").AlignLeft().Padding(10);
            ui.Add(cmT);

            var d = new UITextNameValue("Name", "Text", "Value ");
            d.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/TextNameValue"), true));
            ui.Add(d);

            var img2e = new UIImageTextDescTime(new WebMeta().Put("tag", "I12", "text", "ImageTextDescTime组件", "desc", "desc格式", "time", "time格式", "right", "right").Put("src", "https://data.kukahome.com/css/images/logo.png"));
            img2e.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/ImageTextDescTime"), true));
            ui.Add(img2e);

            UIImageTextValue imageTextValue = new UIImageTextValue("https://data.kukahome.com/css/images/logo.png", "ImageTextValue组件", "Value格式");
            imageTextValue.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/ImageTextValue"), true));
            ui.Add(imageTextValue);


            UI ui3 = new UI("UI组件", "Value格式");
            ui3.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/UI"), true));
            ui3.Icon('\uf013', 0x4CAF50);
            ui.Add(ui3);
            //UIIconNameDesc iconNameDesc = new UIIconNameDesc(new UIIconNameDesc.Item("UI组件", "desc"));
            ui.Add(new UIIconNameDesc(new UIIconNameDesc.Item("UI组件", "desc").Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true))));
            ui.Add(new UIIconNameDesc(new UIIconNameDesc.Item("https://data.kukahome.com/css/images/logo.png", "UI组件", "desc").Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true))));
            ui.Add(new UIIconNameDesc(new UIIconNameDesc.Item("https://data.kukahome.com/css/images/logo.png", "UI组件", "desc")
                .Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true))).Button("关注", Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true), 0x1890ff));

            ui.Add(new UIIconNameDesc(new UIIconNameDesc.Item("https://data.kukahome.com/css/images/logo.png", "UI组件", "desc"), new UIIconNameDesc.Item("https://data.kukahome.com/css/images/logo.png", "UI组件", "desc").Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true))));
            var dis = new UIDiscount(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true));
            dis.Title("UIDiscount优惠券组件");
            dis.State("有效");
            dis.Value("5元");
            dis.Desc("超级优惠券");
            dis.Start("2020.12.1");
            dis.End("2020.12.1");
            ui.Add(dis);


            var look = new UICMSLook("https://data.kukahome.com/css/images/logo.png", "CMSLook组件", "desc");
            look.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true));

            ui.Add(look);
            var v = new UISheet("UISheet组件");
            v.AddItem("CMSLook组件", "desc");
            v.AddItem("CMSLook组件", "desc", true);

            ui.Add(v);

            var uiitems = new UIItems();
            uiitems.Add("https://data.kukahome.com/css/images/logo.png", "Title", "desc", 0xff2, 0xff0000);
            uiitems.Add("https://data.kukahome.com/css/images/logo.png", "Title", "desc", 0xff2, 0xff0000);
            uiitems.Add("https://data.kukahome.com/css/images/logo.png", "Title", "desc", 0xff2, 0xff0000);
            uiitems.Add("https://data.kukahome.com/css/images/logo.png", "UIItems", "UIItems");
            //uiitems.Add("https://data.kukahome.com/css/images/logo.png", "UIItems", "UIItems");
            //dis.Click()
            ui.Add(uiitems);
            UINineImage nineImage = new UINineImage();
            nineImage.Add("https://data.kukahome.com/css/images/logo.png");
            nineImage.Add("https://data.kukahome.com/css/images/logo.png");
            nineImage.Add("https://www.365lu.cn/css/images/center_left.svg");
            nineImage.Click(Web.UIClick.Pager("Subject", "UIData", new UMC.Web.WebMeta().Put("Id", "UMC/Rows/IconNameDesc"), true));
            ui.Add(nineImage);
            ui.Add(new Web.UI.UIIcon().Add(new UIEventText('\ue906', "Iicon").Badge("12323"), new UIEventText("Iicon").Src("https://data.kukahome.com/css/images/logo.png"), new UIEventText("Iicon").Src("https://data.kukahome.com/css/images/logo.png").Badge("1")));

            UITitleMore more = new UITitleMore("Slider");
            var tab = new UITabFixed();
            tab.Add("列组", "1");

            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add("列组", "1");
            tab.Add(new UIClick() { Text = "在的呢" });
            ui.Add(tab);
            var cms = new UICMS(new WebMeta().Put("title", "列组"), videoSrc, "https://data.kukahome.com/css/images/logo.png");
            cms.Left("imy");
            cms.Right("imy");
            ui.Add(cms);
            var cmsImage = new UICMSImage(videoSrc, "https://data.kukahome.com/css/images/logo.png");
            cmsImage.Style.Name("width", "60%").AlignLeft();
            var ui2 = ui.NewSection().Add(more).Add(cmsImage);
            var seilder = new UISlider();
            seilder.Add(videoSrc, "https://data.kukahome.com/css/images/logo.png");
            //seilder.Add("https://data.kukahome.com/css/images/logo.png");/
            seilder.Small();
            ui2.Add(seilder);
            seilder = new UISlider(true);
            seilder.Add("https://data.kukahome.com/css/images/logo.png");
            seilder.Add("https://data.kukahome.com/css/images/logo.png");
            // seilder.Small();
            ui2.Add(seilder);
            seilder = new UISlider();
            seilder.Add("https://data.kukahome.com/css/images/logo.png");
            //seilder.Add("https://data.kukahome.com/css/images/logo.png");
            seilder.Row();
            ui2.Add(seilder);
            UITextItems textItems = new UITextItems();
            textItems.Add(new UIEventText("232323").Style(new UIStyle().BgColor()), new UIEventText("安装").Style(new UIStyle().BgColor()), new UIEventText("安装"), new UIEventText("安装"));
            textItems.Add(new UIEventText("232323"), new UIEventText("安装"), new UIEventText("安装"), new UIEventText("sdsdsfsdfsdfsd").Style(new UIStyle().BgColor()));
            textItems.Add(new UIEventText("232323").Style(new UIStyle().BgColor()), new UIEventText("dsdsd"), new UIEventText("sdsdsd"), new UIEventText("sdsdsd").Style(new UIStyle().BgColor()));

            ui2.Add(textItems);
            UIButton button = new UIButton();
            button.Button(new UIEventText("安装").Badge("1d"));
            button.Style.AlignCenter();
            ui.NewSection().Add(button);
            ui.Title.Right(new UIEventText('\uf2e1', "33d").Click(new UIClick() { Key = "Float" }));//.Badge("21"));
            response.Redirect(ui);
        }
    }
}