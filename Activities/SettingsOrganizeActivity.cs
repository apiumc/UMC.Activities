using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web.UI;
using UMC.Web;
using UMC.Data;
using System.Linq;

namespace UMC.Activities
{
    class SettingsOrganizeActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var sId = Web.UIDialog.AsyncDialog("Id", dkey =>
            {
                var ParentId = UMC.Data.Utility.Guid(Web.UIDialog.AsyncDialog("ParentId", d => this.DialogValue(Guid.Empty.ToString()))).Value;
                var Type = Web.UIDialog.AsyncDialog("Type", k => this.DialogValue("Organize"));
                switch (Type)
                {
                    case "User":
                        response.Redirect(UMC.Data.DataFactory.Instance().Users(new UMC.Data.Entities.Organize { Id = ParentId }));
                        break;
                    default:
                        response.Redirect(UMC.Data.DataFactory.Instance().Organizes(ParentId));
                        break;

                }

                return this.DialogValue(Guid.Empty.ToString());
            });

            if (sId.IndexOf(',') > 0)
            {
                var sids = sId.Split(',');
                for (var i = 0; i < sids.Length; i++)
                {
                    UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Organize { Id = UMC.Data.Utility.Guid(sids[i]).Value, ModifyTime = DateTime.Now, Seq = i });
                }
                this.Context.End();
            }

            var Id = UMC.Data.Utility.Guid(sId);

            var organize = UMC.Data.DataFactory.Instance().Organize(Id ?? Guid.Empty) ?? new UMC.Data.Entities.Organize();




            var userValue = this.AsyncDialog("Settings", d =>
            {
                var fdlg = new Web.UIFormDialog();


                if (organize.Id.HasValue)
                {
                    var org = UMC.Data.DataFactory.Instance().Organize(organize.ParentId ?? Guid.Empty) ?? new UMC.Data.Entities.Organize()
                    {
                        Id = Guid.Empty,
                        Caption = "领层"
                    };

                    fdlg.AddOption("上级组织", "ParentId", org.Id.ToString(), org.Caption)

                    .Command("Settings", "SelectOrganize", new WebMeta().Put("Key", "ParentId").Put("NDep", organize.Id));

                    fdlg.AddText("组织名称", "Caption", organize.Caption);
                    fdlg.Title = "组织设置";
                    if (UMC.Data.DataFactory.Instance().Organizes(organize.Id.Value).Length == 0)
                    {
                        fdlg.AddCheckBox("", "Status", "n").Add("删除此组织", "Del");
                    }
                    fdlg.Submit("确认", this.Context.Request, "Settings.Organize");
                }
                else
                {
                    fdlg.Title = "新建组织";
                    var arg = request.SendValues ?? request.Arguments;
                    var parentId = UMC.Data.Utility.Guid(arg["ParentId"]) ?? Guid.Empty;


                    if (parentId == Guid.Empty)
                    {
                        fdlg.AddTextValue().Put("上级组织", "领层");
                        request.Arguments.Put("ParentId", parentId);
                    }
                    else
                    {

                        var org = UMC.Data.DataFactory.Instance().Organize(parentId);

                        if (org.ParentId == Guid.Empty)
                        {
                            fdlg.AddSelect("上级组织", "ParentId")
                            .Put(org.Caption, org.Id.ToString(), true).Put("领层", Guid.Empty.ToString());
                        }
                        else
                        {

                            request.Arguments.Put("ParentId", parentId);
                            fdlg.AddTextValue().Put("上级组织", org.Caption);

                        }

                    }
                    fdlg.AddText("组织名称", "Caption", organize.Caption);

                    fdlg.Submit("确认", this.Context.Request, "Settings.Organize");
                }




                return fdlg;
            });
            var nore = new UMC.Data.Entities.Organize { Id = organize.Id };
            UMC.Data.Reflection.SetProperty(nore, userValue.GetDictionary());
            if (organize.Id.HasValue)
            {

                if ((userValue["Status"] ?? "").Contains("Del"))
                {
                    if (UMC.Data.DataFactory.Instance().Users(new UMC.Data.Entities.Organize { Id = organize.Id.Value }).Length > 0)
                    {
                        this.Prompt("此组织拥有成员");
                    }
                    else if (UMC.Data.DataFactory.Instance().Organizes(organize.Id.Value).Length > 0)
                    {
                        this.Prompt("此组织拥有下级组织");
                    }
                    else
                    {
                        UMC.Data.DataFactory.Instance().Delete(organize);
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Parent", true), true);
                    }

                }
                else
                {
                    nore.ModifyTime = DateTime.Now;

                    UMC.Data.DataFactory.Instance().Put(nore);
                    this.Prompt("更新成功", false);
                    if (nore.ParentId == organize.ParentId)
                    {
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Caption", organize.Caption), true);

                    }
                    else
                    {
                        this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Top", true), true);

                    }

                }
            }
            else
            {
                if (nore.ParentId.HasValue==false)
                {
                    nore.ParentId = Guid.Empty;
                }
                nore.Id = Guid.NewGuid();

                nore.ModifyTime = DateTime.Now;
                nore.Seq = UMC.Data.Utility.TimeSpan();
                UMC.Data.DataFactory.Instance().Put(nore);
                this.Prompt("添加成功", false);
                this.Context.Send(new UMC.Web.WebMeta().Put("type", "Settings.Organize").Put("Top", nore.ParentId == Guid.Empty), true);

            }

        }

    }
}