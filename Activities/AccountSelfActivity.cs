using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Reflection;
using UMC.Web;
using UMC.Security;

namespace UMC.Activities
{
    public class AccountSelfActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Type = this.AsyncDialog("Type", "Client");
            var user = this.Context.Token.Identity();
            if (user.IsAuthenticated == false)
            {
                response.Redirect(request.Model, "Login");
            }
            var aUser = Data.DataFactory.Instance().User(user.Id.Value);
            var Model = this.AsyncDialog("Model", gkey =>
              {
                  WebMeta form = request.SendValues ?? new UMC.Web.WebMeta();

                  if (form.ContainsKey("limit") == false)
                  {
                      this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                              .RefreshEvent("UI.Setting", "Account.Email", "Account.Mobile", "System.Picture")
                              .Builder(), true);

                  }
                  var account = Security.Account.Create(user.Id.Value);
                  var dic = UMC.Web.UISection.Create();
                  var name = user.Name;
                  switch (Type)
                  {
                      default:
                          dic.Title = new UITitle("账户信息");
                          var imageTextView = new UMC.Web.UI.UIImageTextValue(Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", 4), "头像", "");
                          imageTextView.Style.Name("image-width", "100").Name("image-radius", "10");
                          imageTextView.Click(new UIClick("id", user.Id.ToString(), "seq", "1")
                          { Model = "System", Command = "Picture" });

                          dic.Add(imageTextView);

                          dic.AddCell("昵称", user.Alias, new UIClick(new WebMeta(request.Arguments).Put(gkey, "Alias")).Send(request.Model, request.Command));


                          dic.AddCell('\uf084', "登录账号", name, new UIClick() { Model = "Account", Command = "Password" });
                          break;
                      case "Small":

                          var Discount = new UIHeader.Portrait(Data.WebResource.Instance().ImageResolve(user.Id.Value, "1", 4));


                          Discount.Value(user.Alias);

                          Discount.Time(aUser == null ? "未有签名" : aUser.Signature);

                          var color = 0xfff;
                          Discount.Gradient(color, color);
                          Discount.Click(new UIClick("id", user.Id.ToString(), "seq", "1")
                          { Model = "System", Command = "Picture" });
                          var header = new UIHeader();

                          var style = new UIStyle();
                          header.AddPortrait(Discount);
                          header.Put("style", style);

                          style.Name("value").Color(0x111).Size(18).Click(new UIClick(new WebMeta(request.Arguments).Put(gkey, "Alias")).Send(request.Model, request.Command));
                          style.Name("time").Click(new UIClick(new WebMeta(request.Arguments).Put(gkey, "Signature")).Send(request.Model, request.Command));

                          dic.UIHeader = header;

                          break;
                  }



                  var ac = account[Security.Account.EMAIL_ACCOUNT_KEY];
                  if (ac != null && String.IsNullOrEmpty(ac.Name) == false)
                  {

                      name = ac.Name;

                      int c = name.IndexOf('@');
                      if (c > 0)
                      {
                          var cname = name.Substring(0, c);
                          name = name.Substring(0, 2) + "***" + name.Substring(c);
                      }
                      if ((ac.Flags & Security.UserFlags.UnVerification) == Security.UserFlags.UnVerification)
                      {
                          name = name + "(未验证)";

                      }
                  }
                  else
                  {
                      name = "点击绑定";
                  }

                  var cui = Type == "Small" ? dic : dic.NewSection();

                  cui.AddCell('\uf199', "邮箱", name, new UIClick() { Command = "Email", Model = "Account" });

                  ac = account[Security.Account.MOBILE_ACCOUNT_KEY];
                  if (ac != null && String.IsNullOrEmpty(ac.Name) == false)
                  {
                      name = ac.Name;
                      if (name.Length > 3)
                      {
                          name = name.Substring(0, 3) + "****" + name.Substring(name.Length - 3);
                      }
                      if ((ac.Flags & Security.UserFlags.UnVerification) == Security.UserFlags.UnVerification)
                      {
                          name = name + "(未验证)";
                      }
                  }
                  else
                  {
                      name = "点击绑定";
                  }

                  cui.AddCell('\ue91a', "手机号码", name, new UIClick() { Command = "Mobile", Model = "Account" });

                  switch (Type)
                  {
                      case "Small":
                          response.Redirect(dic);
                          break;
                      default:
                          dic.NewSection().AddCell("个性签名", aUser == null ? "未有签名" : aUser.Signature, new UIClick(new WebMeta(request.Arguments).Put(gkey, "Signature")).Send(request.Model, request.Command));

                          break;
                  }
                  var sess = UMC.Data.DataFactory.Instance().Session(user.Id.Value)
                  .Where(r => String.Equals("Settings", r.ContentType) == false).ToArray();
                  if (sess.Length > 0)
                  {
                      var ui4 = cui.NewSection();
                      ui4.Header.Put("text", "登录设备");
                      foreach (var s in sess)
                      {
                          ui4.AddCell(UMC.Data.Utility.GetDate(s.UpdateTime), s.ContentType);
                      }
                  }

                  if (request.IsApp == false && request.IsWeiXin == false)
                  {
                      UICell cell = UICell.Create("UI", new UMC.Web.WebMeta().Put("text", "退出登录").Put("Icon", "\uf011").Put("click", new UIClick() { Model = "Account", Command = "Close" }));
                      cell.Style.Name("text", new UIStyle().Color(0xf00));
                      dic.NewSection().NewSection().Add(cell);
                  }


                  response.Redirect(dic);
                  return this.DialogValue("none");
              });

            if (aUser == null)
            {
                this.Prompt("第三方账户，不支持修改此内容");
            }
            switch (Model)
            {
                case "Alias":
                    String Alias = this.AsyncDialog("Alias", a => new UITextDialog() { Title = "修改别名", DefaultValue = user.Alias });
                    Membership.Instance().ChangeAlias(user.Name, Alias);
                    this.Prompt(String.Format("您的账户的别名已修改成{0}", Alias), false);
                    this.Context.Send("UI.Setting", true);


                    break;
                case "Signature":

                    var reset = Web.UIFormDialog.AsyncDialog(this.Context, "value", g =>
                    {


                        var selt = new Web.UIFormDialog();
                        selt.Title = "个性签名";
                        selt.AddTextarea("个性签名", "Signature", aUser == null ? "" : aUser.Signature);
                        selt.Submit("确认提交", "UI.Setting");
                        return selt;
                    });
                    var Signature = reset["Signature"];
                    Data.DataFactory.Instance().Put(new UMC.Data.Entities.User { Signature = Signature, Id = user.Id });

                    WebMeta print = new UMC.Web.WebMeta();
                    print["type"] = "UI.Setting";
                    print["Signature"] = Signature;
                    this.Context.Send(print, true);


                    break;
            }

        }
    }
}