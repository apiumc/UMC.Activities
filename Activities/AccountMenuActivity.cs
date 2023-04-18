
using System;
using System.Collections.Generic;
using System.Linq;
using UMC.Data;
using UMC.Web;

namespace UMC.Activities
{
    class AccountMenuActivity : WebActivity
    {
        Dictionary<int, UMC.Security.Identity> pairs = new Dictionary<int, Security.Identity>();
        List<WebMeta> Children(List<UMC.Data.Entities.Menu> menus, int parentId, UMC.Data.WebResource webr)
        {
            var values = new List<WebMeta>();
            var ms = menus.FindAll(d => d.ParentId == parentId);
            menus.RemoveAll(d => d.ParentId == parentId);
            foreach (var p in ms)
            {
                var m = new WebMeta().Put("icon", p.Icon).Put("text", p.Caption).Put("id", p.Id);

                m.Put("src", webr.ImageResolve(new Guid(UMC.Data.Utility.MD5("Menu", p.Site, p.Id.Value)), "1", 0));

                if (menus.Exists(d => d.ParentId == p.Id) && (p.IsHidden ?? false) == false)
                {
                    var childs = Children(menus, p.Id.Value, webr);
                    if (childs.Count > 0)
                    {
                        m.Put("menu", childs);
                        values.Add(m);
                    }
                }
                else if (p.IsHidden ?? false)
                {
                    if (p.ParentId == 0)
                    {
                        if (String.IsNullOrEmpty(p.Url) == false)
                        {
                            if (p.Url[0] == '#')
                            {
                                _Parent["home"] = p.Url.Substring(1);
                            }
                        }

                    }
                }
                else
                {
                    var site = p.Site ?? 0;
                    String path = p.Url;
                    var k = path.IndexOf(':');
                    var strUrl = p.Url;
                    var wk = new WebMeta();
                    if (k > 0)
                    {
                        var svalue = path.Substring(0, k).ToLower();
                        switch (svalue)
                        {
                            case "http":
                            case "https":
                                if (svalue.EndsWith("#_blank"))
                                {
                                    strUrl = strUrl.Substring(0, strUrl.Length - 7);
                                    m.Put("target", "_blank");
                                }
                                path = $"Menu/{p.Id}";
                                break;
                            default:
                                site = UMC.Data.Utility.IntParse(UMC.Data.Utility.Guid(svalue, true).Value);

                                path = path.Substring(k + 1);
                                if (path.StartsWith("//"))
                                {
                                    var callback = Uri.EscapeDataString("/" + path.TrimStart('/'));
                                    strUrl = $"{scheme}://{svalue}{union}{domain}/UMC.Login/Check?callback={callback}";
                                }
                                else
                                {
                                    strUrl = $"{scheme}://{svalue}{union}{domain}/{path.TrimStart('/')}";
                                }
                                path = path.TrimStart('/');
                                break;
                        }


                    }
                    switch (path[0])
                    {
                        case '/':
                        case '#':
                            path = path.Substring(1);
                            break;
                    }
                    k = path.IndexOf('?');
                    if (k > 0)
                    {
                        path = path.Substring(0, k);
                    }

                    if (pairs.ContainsKey(site) == false)
                    {
                        this.pairs[site] = UMC.Security.Identity.Create(User, UMC.Data.DataFactory.Instance().Roles(this.User.Id.Value, site));
                    }

                    if (UMC.Security.AuthManager.IsAuthorization(pairs[site], site, path))
                    {
                        m.Put("url", strUrl).Put("leaf", true);
                        values.Add(m);
                    }
                }

            }
            return values;
        }
        private string domain, union, scheme;//, root;
        private UMC.Security.Identity User;
        private WebMeta _Parent;
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var root = this.AsyncDialog("Site", "0");
            var site = Utility.IntParse(root, -1);
            if (site == -1)
            {
                site = UMC.Data.Utility.IntParse(UMC.Data.Utility.Guid(root, true).Value);
            }

            this.User = this.Context.Token.Identity();
            pairs[0] = this.User;
            this.scheme = request.Url.Scheme;
            union = Data.WebResource.Instance().Provider["union"] ?? ".";
            domain = Data.WebResource.Instance().WebDomain();
            var menus = UMC.Data.DataFactory.Instance().Menu(site).OrderBy(d => d.Seq ?? 0).ToList();

            var webr = UMC.Data.WebResource.Instance();
            _Parent = new WebMeta();
            var parent = Utility.IntParse(this.AsyncDialog("Parent", "0"), 0);
            var menu = Children(menus, parent, webr);
            if (parent != 0)
            {
                var p = UMC.Data.DataFactory.Instance().Menu(site, parent);
                if (p != null)
                {
                    _Parent.Put("text", p.Caption);
                    _Parent.Put("id", p.Caption);
                }
            }
            else if (site != 0)
            {

                if (request.IsMaster == false && pairs.ContainsKey(site) == false)
                {
                    this.pairs[site] = UMC.Security.Identity.Create(User, UMC.Data.DataFactory.Instance().Roles(this.User.Id.Value, site));

                }
                if (request.IsMaster || pairs[site].IsInRole(UMC.Security.AccessToken.AdminRole))
                {
                    var m = new WebMeta("text", "应用设置").Put("icon", "\uf085");
                    var ls = new List<WebMeta>();
                    ls.Add(new WebMeta("text", "应用菜单", "url", "#menu"));
                    ls.Add(new WebMeta("text", "应用账户", "url", "#accout"));
                    ls.Add(new WebMeta("text", "应用授权", "url", "#authority"));
                    menu.Add(m);
                    m.Put("menu", ls);


                }
            }
            _Parent.Put("menu", menu);
            response.Redirect(_Parent);
        }
    }
}