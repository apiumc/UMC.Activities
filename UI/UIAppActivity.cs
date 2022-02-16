using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using UMC.Net;
using UMC.Web;
using System.Collections;
using UMC.Data;

namespace UMC.Activities
{
    class UIAppActivity : Web.WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var config = this.AsyncDialog("Key", g => this.DialogValue("none"));

            switch (config)
            {
                case "Version":
                case "Builder":
                case "json":
                case "Android":
                case "IOS":
                    break;
                default:
                    if (request.IsMaster == false)
                    {
                        this.Prompt("只有管理员才能设置App");
                    }
                    break;
            }
            var file = Reflection.ConfigPath("UMC\\App.json");
            if (System.IO.File.Exists(file) == false)
            {
                using (var stream = UMC.Data.Utility.Writer(file, false))
                {
                    new Uri("http://oss.365lu.cn/UserResources/demo.json").WebRequest().Get().ReadAsStream(stream);
                    stream.Close();
                }
            }

            var appConfig = Data.JSON.Deserialize(Utility.Reader(file)) as Hashtable;

            switch (config)
            {
                case "Version":
                    {

                        int vindex = request.UserAgent.IndexOf("android");
                        if (vindex > 0)
                        {
                            var v = request.UserAgent.Substring(request.UserAgent.IndexOf("V", vindex) + 1);
                            var cfgs = Data.DataFactory.Instance().Configuration("Version");/// UMC.Data.ProviderConfiguration.GetProvider(Reflection.ConfigPath("Version.xml"));
                            if (cfgs == null)
                            {
                                this.Prompt("提示", "当前已经是最新版本");
                            }

                            var provider = cfgs[config];
                            if (provider != null)
                            {
                                if (Data.Utility.IntParse(provider["versionCode"], 0) > UMC.Data.Utility.IntParse(v, 0))
                                {
                                    this.AsyncDialog("Confirm", g =>
                                    {
                                        return new UMC.Web.UIConfirmDialog("您有新版本，点击确认安装") { Title = "新版本安装" };
                                    });
                                    var meta = new UMC.Web.WebMeta();
                                    meta.Put("name", provider.Name);
                                    meta.Put("text", provider["text"]);
                                    meta.Put("title", provider["title"]);
                                    meta.Put("src", provider["src"]).Put("type", "Download");
                                    this.Context.Send(meta, true);

                                }
                                else
                                {
                                    this.Prompt("提示", "当前已经是最新版本");
                                }
                            }
                            else
                            {
                                this.Prompt("提示", "当前已经是最新版本");

                            }
                        }
                        else
                        {
                            this.Prompt("苹果版本自动更新");
                        }
                    }
                    break;
                case "Builder":
                    if (String.IsNullOrEmpty(appConfig["AppName"] as string))
                    {
                        this.Prompt("应用名称不能为空");
                    }
                    if (String.IsNullOrEmpty(appConfig["IconSrc"] as string))
                    {
                        this.Prompt("请上传图标");
                    }
                    if (String.IsNullOrEmpty(appConfig["BgSrc"] as string))
                    {
                        this.Prompt("请上传启动图");
                    }
                    var dataKey = new Hashtable();
                    dataKey["root"] = Utility.GetRoot(request.Url);
                    dataKey["host"] = new Uri(request.Url, "/").AbsoluteUri.Trim('/');
                    appConfig["DataKey"] = dataKey;
                    response.Redirect(appConfig);
                    break;
                case "json":
                    appConfig["IsMaster"] = request.IsMaster;
                    response.Redirect(appConfig);
                    break;
                case "Reset":
                    {
                        var ResetName = this.AsyncDialog("Reset", g =>
                        {
                            var k = new UISelectDialog()
                            {
                                Title = "选择参考的默认的界面架构"
                            };
                            k.Options.Add("DOME架构", "demo");
                            return k;
                        });
                        String stream = new Uri(String.Format("http://oss.365lu.cn/UserResources/{0}.json", ResetName)).WebRequest().Get().ReadAsString();

                        var appConfig2 = Data.JSON.Deserialize(stream) as Hashtable;
                        appConfig2["BgSrc"] = appConfig["BgSrc"];
                        appConfig2["IconSrc"] = appConfig["IconSrc"];
                        appConfig2["AppName"] = appConfig["AppName"];
                        appConfig2["Android"] = appConfig["Android"];
                        appConfig2["IOS"] = appConfig["IOS"];
                        appConfig = appConfig2;
                    }
                    break;
                case "News":
                    {

                        var key = this.AsyncDialog("Sheet", g =>
                          {
                              var k = new UIFormDialog() { Title = "新增Bar" };
                              k.AddText("标题", "text", String.Empty);
                              k.AddOption("图标", "icon").Command("System", "Icon");
                              k.AddCheckBox("", "max", "no").Put("显示大按钮", "true");
                              k.AddRadio("Bar加载类型", "Type")
                                        .Put("默认主页", "Home").Put("电商购物篮", "Cart").Put("电商品类页", "Category").Put("Tabs", "Tab配置页").Put("点击项", "Click")
                                        .Put("基本页", "Pager");
                              k.Submit("确认提交", request, "AppConfig");
                              return k;
                          });
                        var footbar = new ArrayList(appConfig["footBar"] as Array);
                        var data = new WebMeta().Put("key", key["Type"]).Put("text", key["text"]).Put("icon", key["icon"]);
                        if ((key["max"] ?? "").Contains("true"))
                        {
                            data.Put("max", true);
                        }
                        footbar.Add(data);
                        appConfig["footBar"] = footbar;
                    }
                    break;
                case "Android":
                case "IOS":
                    appConfig[config] = this.AsyncDialog("Value", g =>
                    {
                        var k = new UITextDialog() { Title = config + "下载地址", DefaultValue = appConfig[config] as string };
                        return k;
                    });
                    break;
                case "BgSrc":
                case "IconSrc":
                    {

                        var AppName = this.AsyncDialog("Value", g =>
                        {
                            var k = new UITextDialog() { Title = "值" };
                            return k;
                        });
                        var Key = String.Format("UserResources/{0}/{1}/{2}", Utility.GetRoot(request.Url), config, AppName.Substring(AppName.LastIndexOf('/') + 1));
                        var webR = UMC.Data.WebResource.Instance();
                        webR.Transfer(new Uri(AppName), Key);
                        appConfig[config] = new Uri(request.Url, webR.WebDomain() + Key).AbsoluteUri;
                    }
                    break;
                case "AppName":
                    {
                        var AppName = this.AsyncDialog("AppName", g =>
                        {
                            var k = new UITextDialog()
                            {
                                Title = "应用名称",
                                DefaultValue = appConfig["AppName"] as string
                            };
                            return k;
                        });
                        appConfig["AppName"] = AppName;
                    }
                    break;
                case "Del":
                    {
                        var footbar = new ArrayList(appConfig["footBar"] as Array);

                        var index = Utility.IntParse(this.AsyncDialog("Index", "0"), -1);
                        footbar.RemoveAt(index);
                        appConfig["footBar"] = footbar;
                    }
                    break;
                default:
                    {
                        var footbar = new ArrayList(appConfig["footBar"] as Array);
                        var index = Utility.IntParse(this.AsyncDialog("Index", "0"), -1);

                        var hash = footbar[index] as Hashtable;
                        var settings = this.AsyncDialog("Settings", g =>
                        {
                            var fm = new UIFormDialog();
                            fm.Title = "图标";
                            fm.AddText("标题", "text", hash["text"] as string);
                            fm.AddOption("图标", "icon", hash["icon"] as string, hash["icon"] as string).Command("System", "Icon");
                            switch (config)
                            {
                                case "Setting":
                                    switch (hash["key"] as string ?? "Click")
                                    {
                                        case "Home":
                                        case "Category":
                                            break;
                                        case "Cart":
                                            fm.AddCheckBox("", "max", "no").Put("显示大按钮", "true", hash.ContainsKey("max"));
                                            break;
                                        case "Click":
                                            fm.AddCheckBox("", "max", "no").Put("显示大按钮", "true", hash.ContainsKey("max"));
                                            var click = (hash["click"] as Hashtable) ?? new Hashtable();
                                            fm.AddText("事件模块", "model", click["model"] as string);
                                            fm.AddText("事件指令", "cmd", click["cmd"] as string);
                                            fm.AddText("事件参数", "send", click["send"] as string).NotRequired();
                                            break;
                                        case "Tabs":
                                            fm.AddText("数据源模块", "model", hash["model"] as string);
                                            fm.AddText("数据源指令", "cmd", hash["cmd"] as string);
                                            break;
                                        case "Pager":
                                            fm.AddText("加载模块", "model", hash["model"] as string);
                                            fm.AddText("加载指令", "cmd", hash["cmd"] as string);
                                            break;
                                        default:
                                            this.Prompt("固定页面，不支持设置"); ;
                                            break;
                                    }
                                    break;

                            }
                            return fm;
                        });

                        hash["text"] = settings["text"];
                        hash["icon"] = settings["icon"];
                        if ((settings["max"] ?? "").Contains("true"))
                        {
                            hash["max"] = true;
                        }
                        else
                        {
                            hash.Remove("max");
                        }
                        switch (hash["key"] as string)
                        {
                            case "Click":
                                var click = new WebMeta().Put("model", settings["model"])
                                    .Put("cmd", settings["cmd"]);
                                if (String.IsNullOrEmpty(settings["model"]) == false)
                                {
                                    click.Put("send", settings["send"]);
                                }
                                hash["click"] = click;
                                break;
                            case "Cart":
                                break;
                            case "Tabs":
                                hash["model"] = settings["model"];
                                hash["cmd"] = settings["cmd"];
                                break;
                            case "Pager":
                                hash["model"] = settings["model"];
                                hash["cmd"] = settings["cmd"];
                                break;
                        }

                    }
                    break;
            }

            Utility.Writer(file, Data.JSON.Serialize(appConfig), false);
            this.Context.Send("AppConfig", new WebMeta().Put("Config", appConfig), true);
        }
    }
}