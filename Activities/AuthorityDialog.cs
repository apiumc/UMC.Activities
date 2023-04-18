
using System;
using System.Collections;

namespace UMC.Activities
{
    class AuthorityDialog : UMC.Web.UIGridDialog
    {
        public AuthorityDialog(int siteKey)
        {
            this.Title = "授权符";
            this._siteKey = siteKey;
        }
        int _siteKey;
        protected override Hashtable GetHeader()
        {
            var header = new Header("Key", 25);
            header.AddField("Desc", "授权符");

            return header.GetHeader();


        }
        protected override Hashtable GetData(IDictionary paramsKey)
        {
            var start = UMC.Data.Utility.Parse((paramsKey["start"] ?? "0").ToString(), 0);
            var limit = UMC.Data.Utility.Parse((paramsKey["limit"] ?? "25").ToString(), 25);

            var search = new UMC.Data.Entities.Authority() { Site = _siteKey };


            System.Data.DataTable data = new System.Data.DataTable();
            data.Columns.Add("Site");
            data.Columns.Add("Key");
            data.Columns.Add("Desc");

            var Keyword = (paramsKey["Keyword"] as string ?? String.Empty);
            if (String.IsNullOrEmpty(Keyword) == false)
            {
                search.Key = Keyword;
            }
            int next;


            UMC.Data.Utility.Each(Data.DataFactory.Instance().Search(search, start, limit, out next), dr =>
            {
                data.Rows.Add(dr.Site, dr.Key, dr.Desc);
            });

            var hash = new Hashtable();
            hash["data"] = data;
            if (data.Rows.Count == 0 && start == 0)
            {
                if (String.IsNullOrEmpty(search.Key) == false)
                {
                    hash["msg"] = $"未搜索到“{search.Key}”授权路径";
                }
                else
                {
                    hash["msg"] = "未有授权路径";
                }
            }
            else
            {
                hash["start"] = next;
                hash["next"] = true;

            }
            return hash;
        }
    }
}