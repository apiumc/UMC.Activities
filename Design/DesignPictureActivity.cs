
using System;
using System.Collections.Generic;
using UMC.Data;
using UMC.Web;
namespace UMC.Activities
{
    public class DesignPictureActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = this.Context.Token.Identity(); // UMC.Security.Identity.Current;
            var groupId = UMC.Data.Utility.Guid(this.AsyncDialog("id", d =>
            {
                this.Prompt("请传入参数");
                return this.DialogValue(user.Id.ToString());
            }), true) ?? Guid.Empty;

            var Seq = this.AsyncDialog("seq", g =>
            {
                if (request.SendValues != null)
                {
                    return this.DialogValue(request.SendValues["Seq"] ?? "0");
                }
                else
                {
                    return this.DialogValue("0");
                }
            });
            WebResource oosr = WebResource.Instance();//as OssResource;
            var media_id = this.AsyncDialog("media_id", g =>
            {
                if (request.IsApp)
                {
                    var f = Web.UIDialog.CreateDialog("File");
                    f.Config.Put("Submit", new UIClick(new WebMeta(request.Arguments.GetDictionary()).Put(g, "Value"))
                    {
                        Command = request.Command,
                        Model = request.Model
                    });
                    return f;

                }
                else
                {

                    var webr = UMC.Data.WebResource.Instance();
                    var from = new Web.UIFormDialog() { Title = "图片上传" };

                    from.AddFile("选择图片", "media_id", webr.ImageResolve(groupId, "1", 4));

                    from.Submit("确认上传", request, "image");
                    return from;
                }
            });

            var picture = Data.DataFactory.Instance().Picture(groupId) ?? new Data.Entities.Picture { group_id = groupId };
            var index = new List<byte>();
            if (picture.Value != null)
            {
                index.AddRange(index);
            }
            if (String.Equals(media_id, "none"))
            {
                var seq = UMC.Data.Utility.Parse(Seq, 0);
                if (request.IsApp == false)
                    this.AsyncDialog("Confirm", s =>
                    {

                        return new Web.UIConfirmDialog(String.Format("确认删除此组第{0}张图片吗", seq)) { Title = "删除提示" };

                    });

                if (seq == 1)
                {
                    if (index.Count > 1)
                    {
                        oosr.Transfer(new Uri(oosr.ImageResolve(picture.group_id.Value, index[1], 0)), groupId, seq);
                    }
                    index.RemoveAt(1);
                }
                else
                {
                    index.Remove(Convert.ToByte(seq));
                }
                if (index.Count == 0)
                {
                    Data.DataFactory.Instance().Delete(picture);
                }
                else
                {
                    picture.Value = index.ToArray();
                    Data.DataFactory.Instance().Put(picture);
                }

            }
            else
            {
                var type = this.AsyncDialog("type", g => this.DialogValue("jpg"));
                var seq = UMC.Data.Utility.Parse(Seq, -1);
                if (media_id.StartsWith("http://") || media_id.StartsWith("https://"))
                {
                    var url = new Uri(media_id);
                    if (url.Host.StartsWith("oss."))
                    {
                        if (seq > -1)
                        {
                            if (seq < 1)
                            {

                                if (index.Count > 0)
                                {

                                    index.Add(Convert.ToByte(index[index.Count - 1] + 1));
                                }
                                else
                                {
                                    index.Add(1);
                                }

                                picture.Value = index.ToArray();
                                Data.DataFactory.Instance().Put(picture);
                            }

                        }
                        if (url.AbsolutePath.EndsWith(type, StringComparison.CurrentCultureIgnoreCase))
                        {
                            oosr.Transfer(url, groupId, seq, type);
                        }
                        else
                        {

                            oosr.Transfer(new Uri(String.Format("{0}?x-oss-process=image/format,{1}", media_id, type)), groupId, seq, type);
                        }
                    }
                    else
                    {

                        if (seq < 1)
                        {
                            if (index.Count > 0)
                            {

                                index.Add(Convert.ToByte(index[index.Count - 1] + 1));
                            }
                            else
                            {
                                index.Add(1);
                            }

                            picture.Value = index.ToArray();
                            Data.DataFactory.Instance().Put(picture);
                        }
                        oosr.Transfer(new Uri(media_id), groupId, seq);
                    }

                }
            }

            this.Context.Send(new WebMeta().Put("type", "image").Put("id", groupId.ToString()), true);


        }


    }

}