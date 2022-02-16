using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Activities
{
    /// <summary>
    /// 
    /// </summary>
    [Mapping("Settings", "SelectEmploye", Auth = WebAuthType.User, Desc = "选择员工", Category = 1)]
    public class SettingsSelectEmployeActivity : WebActivity
    {



        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

        }

    }
}
