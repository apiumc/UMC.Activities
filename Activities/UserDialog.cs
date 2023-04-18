
using System;
using System.Collections;

namespace UMC.Activities
{
    class UserDialog : UMC.Web.UIGridDialog
    {
        public UserDialog()
        {
            this.Title = "账户管理";

        }
        protected override Hashtable GetHeader()
        {
            var header = new Header("Id", 25);
            header.AddField("Username", "登录名");
            header.AddField("Alias", "别名");
            return header.GetHeader();


        }
        protected override Hashtable GetData(IDictionary paramsKey)
        {
            var start = UMC.Data.Utility.Parse((paramsKey["start"] ?? "0").ToString(), 0);
            var limit = UMC.Data.Utility.Parse((paramsKey["limit"] ?? "25").ToString(), 25);


            string sort = paramsKey[("sort")] as string;
            string dir = paramsKey[("dir")] as string;
            var search = new UMC.Data.Entities.User();

            if (!String.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Disabled":
                        search.IsDisabled = true;
                        break;
                    case "Lock":
                        search.Flags = Security.UserFlags.Lock;
                        break;

                }
            }

            System.Data.DataTable data = new System.Data.DataTable();
            data.Columns.Add("Id");
            data.Columns.Add("Username");
            data.Columns.Add("Alias");
            data.Columns.Add("RegistrTime");

            var Keyword = (paramsKey["Keyword"] as string ?? String.Empty);
            if (String.IsNullOrEmpty(Keyword) == false)
            {
                search.Alias = Keyword;
                search.Username = Keyword;
            }
            int next = 0;


            UMC.Data.Utility.Each(Data.DataFactory.Instance().Search(search, start, limit, out next), dr =>
            {
                data.Rows.Add(dr.Id, dr.Username, dr.Alias, UMC.Data.Utility.GetDate(dr.RegistrTime));

            });

            var hash = new Hashtable();
            hash["data"] = data;
            if (next < 0)
            {
                hash["msg"] = "未有对应账户";
            }
            else
            {
                hash["start"] = next;
                hash["total"] = next + 1;

            }
            return hash;
        }
    }
}