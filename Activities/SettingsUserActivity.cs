using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using UMC.Web;
using UMC.Data.Entities;

namespace UMC.Activities
{

    class SettingsUserActivity : WebActivity
    {

        void Setting(User user)
        {
            var userValue = Web.UIFormDialog.AsyncDialog(this.Context, "User", d =>
            {
                var fdlg = new Web.UIFormDialog();

                fdlg.Title = "状态设置";

                var opts2 = new Web.ListItemCollection();

                opts2.Add("别名", user.Alias);
                opts2.Add("登录名", user.Username);
                if (user.ActiveTime.HasValue)
                    opts2.Add("最后登录", String.Format("{0:yy-MM-dd HH:mm}", user.ActiveTime));
                if (user.RegistrTime.HasValue)
                    opts2.Add("注册时间", String.Format("{0:yy-MM-dd HH:mm}", user.RegistrTime));

                fdlg.AddTextValue(opts2);


                var flag = user.Flags ?? UMC.Security.UserFlags.Normal;
                var opts = new Web.ListItemCollection();
                var selected = ((flag & UMC.Security.UserFlags.Lock)) > 0;
                opts.Add("锁定", "Lock", selected);
                opts.Add("禁用", "Disabled", user.IsDisabled ?? false);
                fdlg.AddCheckBox("状态", "Flags", opts, "0");

                fdlg.Submit("确认提交", "User.Change");
                return fdlg;
            });
            var flags = user.Flags ?? UMC.Security.UserFlags.Normal;
            if (userValue["Flags"].Contains("Lock"))
            {
                flags |= Security.UserFlags.Lock;
            }
            else
            {
                if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock)
                {
                    flags ^= Security.UserFlags.Lock;
                }
            }
            var us = new Data.Entities.User { Flags = flags, Username = user.Username, Id = user.Id };
            us.IsDisabled = userValue["Flags"].Contains("Disabled");

            ClearSession(user.Id.Value);
            Data.DataFactory.Instance().Put(us);

            this.Context.Send("User.Change", false);
            this.Prompt("设置成功");
        }
        void ClearSession(Guid userid)
        {

            var sesions = UMC.Data.DataFactory.Instance().Session(userid);
            foreach (var v in sesions)
            {
                switch (v.ContentType)
                {
                    case "Settings":
                        break;
                    default:
                        UMC.Data.DataFactory.Instance().Delete(v);
                        break;
                }
            }
        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var strUser = this.AsyncDialog("Id", d =>
            {
                var dlg = new UserDialog();
                dlg.IsSearch = true;
                if (request.IsMaster)
                {
                    dlg.Menu("创建", "Settings", "User", Guid.Empty.ToString());
                }
                dlg.RefreshEvent = "User.Change";
                return dlg;
            });
            var userId = UMC.Data.Utility.Guid(strUser) ?? Guid.Empty;

            var setting = Web.UIDialog.AsyncDialog(this.Context, "Setting", d =>
             {
                 if (userId == Guid.Empty)
                 {
                     return this.DialogValue("Create");
                 }
                 var form = request.SendValues ?? new WebMeta();
                 if (form.ContainsKey("limit") == false)
                 {
                     this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta().Put("Id", strUser))
                         .RefreshEvent("User.Change")
                         .Builder(), true);

                 }
                 var user = Data.DataFactory.Instance().User(userId);


                 var ui = UISection.Create(new UITitle("用户信息"));


                 ui.AddCell("别名", user.Alias, new UIClick(new WebMeta(request.Arguments).Put(d, "Alias")).Send(request.Model, request.Command));

                 ui.AddCell("账户", user.Username);

                 if (user.ActiveTime.HasValue)
                     ui.AddCell("最后登录", String.Format("{0:yy-MM-dd HH:mm}", user.ActiveTime));
                 if (user.RegistrTime.HasValue)
                     ui.AddCell("注册时间", String.Format("{0:yy-MM-dd HH:mm}", user.RegistrTime));


                 var status = "正常";
                 var flags = user.Flags ?? UMC.Security.UserFlags.Normal;
                 var opts = new Web.ListItemCollection();
                 if (user.IsDisabled == true)
                 {
                     status = "禁用";
                 }
                 else if ((int)(flags & UMC.Security.UserFlags.Lock) > 0)
                 {
                     status = "锁定";
                 }

                 var ll = ui.NewSection()
                   .AddCell("状态", status, new UIClick(new WebMeta(request.Arguments).Put(d, "Status")).Send(request.Model, request.Command));


                 if (String.IsNullOrEmpty(Data.DataFactory.Instance().Password(user.Id.Value)))
                 {
                     ll.AddCell("口令", "未开通", new UIClick(new WebMeta(request.Arguments).Put(d, "Passwrod")).Send(request.Model, request.Command));
                 }
                 else
                 {
                     ll.AddCell("口令", "已开通", new UIClick(new WebMeta(request.Arguments).Put(d, "NoPasswrod")).Send(request.Model, request.Command));
                 }


                 var roes = Data.DataFactory.Instance().Roles(user.Id.Value, 0);

                 var ui2 = ui.NewSection();
                 ui2.AddCell("拥有角色", "设置", new UIClick(new WebMeta(request.Arguments).Put(d, "Role")).Send(request.Model, request.Command));
                 foreach (var dr in roes)
                 {
                     switch (dr)
                     {
                         case UMC.Security.Membership.GuestRole:
                             break;
                         case UMC.Security.Membership.AdminRole:
                             ui2.Delete(UICell.UI('\uf0c0', "超级管理员", dr), new UIEventText()
                             .Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Rolename").Put("Rolename", dr).Put("Site", 0)).Send(request.Model, request.Command)));

                             break;
                         case UMC.Security.Membership.UserRole:
                             ui2.Delete(UICell.UI('\uf0c0', "内部员工", dr), new UIEventText()
                            .Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Rolename").Put("Rolename", dr).Put("Site", 0)).Send(request.Model, request.Command)));

                             break;
                         default:
                             ui2.Delete(UICell.UI('\uf0c0', dr, ""), new UIEventText()
                            .Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Rolename").Put("Rolename", dr).Put("Site", 0)).Send(request.Model, request.Command)));

                             break;
                     }
                 }
                 if (ui2.Length == 1)
                 {
                     ui2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "只拥有来宾角色").Put("icon", "\uF016"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),

                     new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));


                 }

                 var Organize = UMC.Data.DataFactory.Instance().Organizes(new User { Id = user.Id.Value });

                 var ui4 = ui.NewSection();

                 ui4.AddCell("所属组织", "设置", new UIClick(new WebMeta(request.Arguments).Put(d, "ToOrganize")).Send(request.Model, request.Command));


                 if (Organize.Length > 0)
                 {
                     foreach (var s in Organize)
                     {
                         if (request.IsMaster)
                         {
                             ui4.Delete(UICell.Create("UI", new WebMeta().Put("text", s.Caption).Put("Icon", "\uf0e8")), new UIEventText()
                             .Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Organize").Put("OrganizeId", s.Id)).Send(request.Model, request.Command)));
                         }
                         else
                         {
                             ui4.Add(UICell.Create("UI", new WebMeta().Put("text", s.Caption).Put("Icon", "\uf0e8")));

                         }
                     }
                 }
                 else
                 {
                     ui4.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未加入组织").Put("icon", "\uf0e8"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),

                     new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));


                 }


                 var accounts = UMC.Data.DataFactory.Instance().Account(user.Id.Value);
                 if (accounts.Length > 0)
                 {
                     var um = ui.NewSection();
                     um.Header.Put("text", "第三方账户");

                     foreach (var ac in accounts)
                     {
                         switch (ac.Type)
                         {
                             case UMC.Security.Account.EMAIL_ACCOUNT_KEY:
                                 um.AddCell('\uf199', "邮箱", ac.Name);
                                 break;
                             case UMC.Security.Account.MOBILE_ACCOUNT_KEY:
                                 um.AddCell('\ue91a', "手机号码", ac.Name);
                                 break;
                             case UMC.Configuration.WeiXin.ACCOUNT_KEY:
                                 um.AddCell('\uf0ce', "微信公众号", "已对接");
                                 break;
                             case UMC.Configuration.DingTalk.ACCOUNT_KEY:
                                 um.AddCell('\uf0ce', "钉钉", "已对接");
                                 break;
                             case UMC.Configuration.Corp.ACCOUNT_KEY:
                                 um.AddCell('\uf0ce', "微信企业号", "已对接");
                                 break;
                             default:
                                 um.AddCell('\uf0ce', "第三方", ac.Name);
                                 break;
                         }

                     }
                 }
                 var sess = UMC.Data.DataFactory.Instance().Session(user.Id.Value)
                 .Where(r => String.Equals("Settings", r.ContentType) == false).ToArray();

                 if (sess.Length > 0)
                 {
                     var ui5 = ui.NewSection();
                     ui5.Header.Put("text", "登录会话");
                     foreach (var s in sess)
                     {
                         ui5.Delete(UICell.Create("UI", new WebMeta().Put("value", UMC.Data.Utility.GetDate(s.UpdateTime), "text", s.ContentType)
                     .Put("Icon", "\uf286")), new UIEventText()
                  .Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Seesion").Put("SessionKey", s.SessionKey)).Send(request.Model, request.Command)));


                     }
                 }

                 ui.UIFootBar = new UIFootBar() { IsFixed = true };
                 ui.UIFootBar.AddText(new UIEventText("重置密码").Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Passwrod")).Send(request.Model, request.Command)),
                     new UIEventText("功能授权").Click(new UIClick(new WebMeta(request.Arguments).Put(d, "Auth")).Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));
                 response.Redirect(ui);

                 return this.DialogValue("none");
             });
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理账户");
            }
            switch (setting)
            {
                case "OrganizeMember":
                    {

                        var OrganizeId = UMC.Data.Utility.IntParse(this.AsyncDialog("OrganizeId", "0"), 0);// ?? Guid.Empty;

                        var sids = strUser.Split(',');
                        for (var i = 0; i < sids.Length; i++)
                        {
                            UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.OrganizeMember
                            {
                                user_id = UMC.Data.Utility.Guid(sids[i]).Value,
                                ModifyTime = DateTime.Now,
                                Seq = i,
                                org_id = OrganizeId
                            });
                        }
                        this.Context.End();
                    }
                    break;
                case "Seesion":
                    {
                        var SessionKey = this.AsyncDialog("SessionKey", r => this.DialogValue(Guid.Empty.ToString()));
                        Data.DataFactory.Instance().Delete(new Session { user_id = userId, SessionKey = SessionKey });

                        this.Context.Send("User.Change", true);
                    }
                    break;

                case "Rolename":
                    {
                        var Rolename = this.AsyncDialog("Rolename", r => this.DialogValue("none"));
                        Data.DataFactory.Instance().Delete(new UserToRole { user_id = userId, Rolename = Rolename, Site = 0 });

                        ClearSession(userId);
                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "Organize":
                    {
                        var OrganizeId = UMC.Data.Utility.IntParse(this.AsyncDialog("OrganizeId", "0"), 0);// ?? Guid.Empty;
                        Data.DataFactory.Instance().Delete(new OrganizeMember { user_id = userId, org_id = OrganizeId });

                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "ToOrganize":
                    {
                        var OrganizeId = UMC.Data.Utility.IntParse(
                        this.AsyncDialog("OrganizeId", "Settings", "SelectOrganize"), 0);

                        Data.DataFactory.Instance().Put(new OrganizeMember
                        {
                            user_id = userId,
                            org_id = OrganizeId,
                            Seq = UMC.Data.Utility.TimeSpan(),
                            ModifyTime = DateTime.Now,
                            MemberType = 0
                        });
                        this.Context.Send("User.Change", true);

                    }
                    break;
                case "Role":
                    {

                        var rolename = this.AsyncDialog("Rolename", "Settings", "SelectRole", new WebMeta().Put("Site", 0));
                        Data.DataFactory.Instance().Put(new UserToRole
                        {
                            user_id = userId,
                            Site = 0,
                            Rolename = rolename
                        });

                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "Status":
                    {
                        this.Setting(Data.DataFactory.Instance().User(userId));
                    }
                    break;
                case "Auth":
                    {
                        var user = Data.DataFactory.Instance().User(userId);
                        response.Redirect("Settings", "Auth", new UMC.Web.WebMeta().Put("Type", "User", "Value", user.Username).Put("Site", 0), true);
                    }
                    break;
                case "Alias":
                    {
                        var user = Data.DataFactory.Instance().User(userId);
                        var users = this.AsyncDialog("User", d =>
                        {
                            var fmDg = new Web.UIFormDialog();

                            var opts = new Web.ListItemCollection();

                            fmDg.Title = "变更别名";
                            opts.Add("登录名", user.Username);
                            fmDg.AddText("新别名", "Alias", user.Alias);
                            fmDg.Submit("确认提交", "User.Change");
                            return fmDg;
                        });

                        UMC.Security.Membership.Instance().ChangeAlias(user.Username, users["Alias"]);
                        this.Prompt(String.Format("{0}的别名已重置", user.Username, users["Alias"]), false);

                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "NoPasswrod":
                    {

                        this.AsyncDialog("Confirm", g => new UIConfirmDialog("确认移除此账户的口令登录吗"));
                        var user = Data.DataFactory.Instance().User(userId) ?? new User();
                        UMC.Data.DataFactory.Instance().Delete(new Password { Key = userId });

                        this.Prompt(String.Format("已经移除{0}密码登录", user.Alias), false);

                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "Passwrod":
                    {
                        var user = Data.DataFactory.Instance().User(userId) ?? new User();
                        var users = this.AsyncDialog("User", d =>
                        {
                            var opts = new Web.ListItemCollection();
                            var fmDg = new Web.UIFormDialog();
                            fmDg.Title = "重置密码";
                            opts.Add("别名", user.Alias);
                            opts.Add("登录名", user.Username);
                            fmDg.AddTextValue(opts);
                            fmDg.AddPassword("密码", "Password", true);
                            fmDg.Submit("确认提交", "User.Change");

                            return fmDg;
                        });

                        UMC.Security.Membership.Instance().Password(user.Username, users["Password"]);
                        this.Prompt(String.Format("{0}的密码已重置", user.Alias), false);

                        this.Context.Send("User.Change", true);
                    }
                    break;
                case "Create":
                    {
                        var OrganizeId = UMC.Data.Utility.IntParse(this.AsyncDialog("OrganizeId", "0"), 0);// ?? Guid.Empty;

                        var users = this.AsyncDialog("User", d =>
                          {
                              var fmDg = new Web.UIFormDialog();

                              fmDg.Title = "新增账户";
                              if (OrganizeId != 0)
                              {
                                  var org = UMC.Data.DataFactory.Instance().Organize(OrganizeId);

                                  if (org != null)
                                  {
                                      fmDg.AddTextValue().Put("所属组织", org.Caption);
                                  }
                              }
                              fmDg.AddText("账户名", "Username", String.Empty);
                              fmDg.AddText("别名", "Alias", String.Empty);
                              fmDg.Submit("确认提交", "User.Change");
                              return fmDg;
                          });
                        if (userId != Guid.Empty)
                        {
                            var uid = UMC.Security.Membership.Instance().CreateUser(userId, users["Username"].Trim(), users["Alias"]);
                            if (uid == null)
                            {
                                this.Prompt(String.Format("已经存在{0}用户名", users["Username"]));
                            }
                        }
                        else
                        {
                            userId = UMC.Security.Membership.Instance().CreateUser(users["Username"].Trim(), users["Alias"]);
                            if (userId == Guid.Empty)
                            {
                                this.Prompt(String.Format("已经存在{0}用户名", users["Username"]));
                            }
                            else
                            {
                                UMC.Security.Membership.Instance().AddRole(users["Username"].Trim(), 0, UMC.Security.Membership.UserRole);
                            }
                        }
                        if (OrganizeId != 0)
                        {
                            UMC.Data.DataFactory.Instance().Put(new OrganizeMember
                            {
                                user_id = userId,
                                org_id = OrganizeId,
                                MemberType = 0,
                                ModifyTime = DateTime.Now,
                                Seq = UMC.Data.Utility.TimeSpan()
                            });
                        }

                        this.Context.Send("User.Change", true);

                    }
                    break;
            }

        }

    }
}