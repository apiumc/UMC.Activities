using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Web;
using UMC.Data;
using UMC.Security;

namespace UMC.Activities
{
    class SettingsAuthActivity : WebActivity
    { 
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var roles = UMC.Data.DataFactory.Instance().Roles();
            var RoleType = UMC.Web.UIDialog.AsyncDialog("Type", d =>
            {
                if (roles.Length < 4)
                {
                    return Web.UIDialog.ReturnValue("User");
                }
                var rd = new Web.UIRadioDialog() { Title = "选择设置账户类型" };
                rd.Options.Add("角色", "Role");
                rd.Options.Add("用户", "User");
                rd.Options.Add("组织", "Organize");
                return rd;
            }); 

            var setValue = UMC.Web.UIDialog.AsyncDialog("Value", d =>
            {
                if (RoleType == "Role")
                {
                    var rd = new Web.UIRadioDialog() { Title = "请选择设置权限的角色" };


                    Utility.Each(roles,
                        dr =>
                        {
                            switch (dr.Rolename)
                            {
                                case UMC.Security.Membership.AdminRole:
                                case UMC.Security.Membership.GuestRole:
                                    break;
                                default:
                                    rd.Options.Add(dr.Rolename, dr.Rolename);
                                    break;
                            }
                        });
                    return rd;
                }
                else
                {
                    return new UserDialog() { Title = "请选择设置权限的账户" };
                }
            });

            var wdcks = Web.WebServlet.Auths();

            var ids = new List<String>();
            Utility.Each(wdcks, g => ids.Add(g.Get("key")));
            if (wdcks.Count == 0)
            {
                this.Prompt("现在的功能不需要设置权限");
            }
            var wdks = new List<UMC.Data.Entity<UMC.Data.Entities.Wildcard, List<UMC.Security.Authorize>>>();



            Utility.Each(UMC.Data.DataFactory.Instance().Wildcard(ids.ToArray()), dr =>
            {
                wdks.Add(new Data.Entity<Data.Entities.Wildcard, List<Security.Authorize>>(dr, dr.Authorizes));
            });


            var Wildcard = Web.UIDialog.AsyncDialog("Wildcards", d =>
            {
                var fmdg = new Web.UICheckboxDialog();
                fmdg.Title = "权限设置";
                fmdg.DefaultValue = "None";


                foreach (var cm in wdcks)
                {
                    var id = cm.Get("key");

                    var wdk = wdks.Find(w => String.Equals(w.Value.WildcardKey, id, StringComparison.CurrentCultureIgnoreCase));
                    if (wdk != null)
                    {
                        if (wdk.Config != null)
                        {
                            var isS = false;
                            switch (RoleType)
                            {
                                case "Organize":

                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.OrganizeDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;
                                case "Role":
                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.RoleDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;
                                default:
                                    isS = wdk.Config.Exists(a => a.Type == Security.AuthorizeType.UserDeny
                                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));
                                    break;

                            }
                            fmdg.Options.Add(cm.Get("desc"), id, !isS);
                        }
                        else
                        {
                            fmdg.Options.Add(cm.Get("desc"), id, true);
                        }
                    }
                    else
                    {
                        fmdg.Options.Add(cm.Get("desc"), id, true);
                    }
                }

                return fmdg;

            });
            foreach (var cm in wdcks)
            {
                var id = cm.Get("key");
                var wdk = wdks.Find(w => String.Equals(w.Value.WildcardKey, id, StringComparison.CurrentCultureIgnoreCase));

                List<Security.Authorize> authorizes;
                if (wdk != null)
                {
                    authorizes = wdk.Config;
                }
                else
                {
                    authorizes = new List<Security.Authorize>();
                }
                switch (RoleType)
                {
                    case "Role":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.RoleDeny || a.Type == Security.AuthorizeType.RoleAllow)
                      && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                    case "Organize":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.OrganizeAllow || a.Type == Security.AuthorizeType.OrganizeDeny)
                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                    case "User":
                        authorizes.RemoveAll(a => (a.Type == Security.AuthorizeType.UserAllow || a.Type == Security.AuthorizeType.UserDeny)
                        && String.Equals(a.Value, setValue, StringComparison.CurrentCultureIgnoreCase));

                        break;
                }
                if (Wildcard.IndexOf(id) == -1)
                {
                    switch (RoleType)
                    {
                        case "Role":
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.RoleDeny });

                            break;
                        case "Organize":
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.OrganizeDeny });

                            break;
                        default:
                            authorizes.Add(new Security.Authorize { Value = setValue, Type = Security.AuthorizeType.UserDeny });

                            break;
                    }

                    UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Wildcard
                    {
                        Authorizes = UMC.Data.JSON.Serialize(authorizes),
                        WildcardKey = id,
                        Description = cm.Get("desc")
                    });
                }

            }
            this.Prompt("权限设置成功");

        }
    }
}