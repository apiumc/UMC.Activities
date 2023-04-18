
using System;
using UMC.Activities.Entities;
using System.Linq;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignConfigActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            String group = request.Command;


            Guid? vid = UMC.Data.Utility.Guid(this.AsyncDialog("Id", s =>
            {
                UIGridDialog rdoDig = UIGridDialog.Create(new UIGridDialog.Header("Id", 0)
                                .PutField("Name", "标题").PutField("Value", "代码")
                        , DataFactory.Instance().DesignConfig(this.Context.AppKey ?? Guid.Empty, group));
                rdoDig.Menu("新建配置", request.Model, request.Command, "News");
                rdoDig.RefreshEvent = "Settings";
                rdoDig.Title = "数据配置";


                return rdoDig;
            }));// ??Guid.Empty;

            WebMeta configs = this.AsyncDialog(s =>
            {
                UIFormDialog fm = new UIFormDialog();
                if (vid.HasValue == false)
                {
                    fm.Title = "新增配置值";
                }
                else
                {
                    fm.Title = "修改配置值";
                }
                PageConfig con = null;
                if (vid.HasValue)
                {
                    con = DataFactory.Instance().DesignConfig(vid.Value);
                }
                if (con == null)
                {
                    var max = DataFactory.Instance().DesignConfig(this.Context.AppKey ?? Guid.Empty, group).MAX(r => r.Sequence ?? 0);
                    con = new PageConfig() { Sequence = max };
                }

                fm.AddText("配置名称", "Name", con.Name);
                fm.AddText("配置标题", "Value", con.Value);
                fm.AddNumber("显示顺序", "Sequence", con.Sequence);
                return fm;
            }, "Config");
            PageConfig cv = new PageConfig();
            UMC.Data.Reflection.SetProperty(cv, configs.GetDictionary());
            if (vid.HasValue == false)
            {
                cv.GroupBy = group;
                cv.Id = Guid.NewGuid();
                cv.AppKey = this.Context.AppKey ?? Guid.Empty;
                DataFactory.Instance().Put(cv);
            }
            else
            {
                cv.Id = vid;
                if (cv.Sequence == -1)
                {
                    DataFactory.Instance().Delete(new PageConfig() { Id = (vid) });

                }
                else
                {
                    DataFactory.Instance().Put(cv);
                }
            }
            this.Context.Send("Settings", true);
        }
    }

}