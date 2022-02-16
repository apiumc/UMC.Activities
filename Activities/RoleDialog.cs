

using System.Collections;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    public class RoleDialog : UIGridDialog
    {

        protected override Hashtable GetHeader()
        {
            IsAsyncData = true;

            Header header = new Header("Rolename", 25);
            header.AddField("Rolename", "角色名");
            return header.GetHeader();
        }

        protected override Hashtable GetData(IDictionary paramsKey)
        {

            var hash = new Hashtable();
            hash["data"] = UMC.Data.DataFactory.Instance().Roles();
            return hash;
        }
    }
}
