
using System;
using System.Collections.Generic;
using System.Linq;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class AccountMenuActivity : WebActivity
    {
        List<WebMeta> Children(List<UMC.Data.Entities.Menu> menus, Guid parentId, bool isMarster, UMC.Data.WebResource webr)
        {

            var values = new List<WebMeta>();
            foreach (var p in menus.FindAll(d => d.ParentId == parentId))
            {
                var m = new WebMeta().Put("icon", p.Icon).Put("text", p.Caption).Put("id", p.Id);
                if (p.IsDisable == true)
                {
                    m.Put("disable", true);
                }
                m.Put("src", webr.ImageResolve(p.Id.Value, "1", 0));
                switch (p.Type ?? 0)
                {
                    case 1:
                        {
                            var childs = Children(menus, p.Id.Value, isMarster, webr);
                            if (childs.Count > 0)
                            {
                                m.Put("menu", childs);
                                values.Add(m);
                            }
                            else if (isMarster)
                            {

                                values.Add(m);
                            }
                        }
                        break;
                    case 0:
                        {
                            var childs = Children(menus, p.Id.Value, isMarster, webr);
                            if (childs.Count > 0)
                            {
                                m.Put("menu", childs);
                            }
                            else
                            {
                                m.Put("url", p.Url).Put("leaf", true); ;
                            }
                            values.Add(m);
                        }
                        break;
                    default:

                        m.Put("url", p.Url).Put("leaf", true);
                        values.Add(m);
                        break;
                }

            }
            return values;
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var site = Utility.IntParse(this.AsyncDialog("Site", "0"), 0);
            var ids = new List<String>();
            var menus = new List<UMC.Data.Entities.Menu>();

            if (request.IsMaster)
            {
                menus = UMC.Data.DataFactory.Instance().Menu(site).OrderBy(d => d.Seq ?? 0).ToList();
            }
            else
            {
                UMC.Data.DataFactory.Instance().Menu(site).Where(r => (r.IsDisable ?? false) == false).OrderBy(d => d.Seq ?? 0).Any(dr =>
                {
                    switch (dr.Type ?? 0)
                    {
                        case 1:
                            break;
                        default:
                            ids.Add(dr.Id.ToString());
                            break;
                    }
                    menus.Add(dr);
                    return false;
                });
                var auths = Security.AuthManager.IsAuthorization(ids.ToArray());
                menus.RemoveAll(p =>
                {
                    switch (p.Type ?? 0)
                    {
                        case 1:
                            break;
                        default:
                            if (auths[ids.IndexOf(p.Id.ToString())] == false)
                            {
                                return true;

                            }
                            break;
                    }
                    return false;
                });

            }
            var webr = UMC.Data.WebResource.Instance();//
            var menu = Children(menus, Guid.Empty, request.IsMaster, webr);

            response.Redirect(menu);
        }
    }
}