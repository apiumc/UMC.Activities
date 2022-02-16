using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    class UISearchActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = UMC.Security.Identity.Current;
            var form = request.SendValues ?? new UMC.Web.WebMeta();

            if (form.ContainsKey("limit"))
            {

                UISection ui = UISection.Create();


                var hot = new UMC.Web.UI.UITextItems();
                ui.NewSection().Add(hot).Header.Put("text", "热门搜索");

                var history = new UMC.Web.UI.UITextItems(request.Model, request.Command);
                history.Event("SearchFor");
                ui.NewSection().Add(history).Header.Put("text", "历史搜索");


                Data.Utility.Each(Data.DataFactory.Instance().SearchKeyword(Guid.Empty), dr => hot.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));


                response.Redirect(ui);
            }

            if (String.IsNullOrEmpty(request.SendValue))
            {

                var history = new List<UIEventText>();

                Data.Utility.Each(Data.DataFactory.Instance().SearchKeyword(user.Id.Value), dr => history.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));


                var hash = new System.Collections.Hashtable();

                hash["data"] = history;
                if (history.Count == 0)
                {
                    hash["msg"] = "请搜索";
                }
                response.Redirect(hash);
            }
            else
            {
                var vs = request.SendValue.Split(',', ' ', '　');

                foreach (var i in vs)
                {
                    if (String.IsNullOrEmpty(i) == false)
                    {
                        var search = new SearchKeyword { Keyword = i, user_id = user.Id, Time = UMC.Data.Utility.TimeSpan() };

                        Data.DataFactory.Instance().Put(search);
                    }
                }

                var history = new List<UIEventText>();
                Data.Utility.Each(Data.DataFactory.Instance().SearchKeyword(user.Id.Value), dr => history.Add(new UIEventText(dr.Keyword).Click(new UIClick(dr.Keyword) { Key = "SearchFor" })));
                var hash = new System.Collections.Hashtable();
                hash["data"] = history;
                response.Redirect(hash);

            }
        }

    }
}