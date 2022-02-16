using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Web.UI;
using UMC.Web;
using UMC.Data.Entities;
using UMC.Data;

namespace UMC.Activities
{
    [Mapping("Settings", "Area", Auth = WebAuthType.All, Desc = "选择地址区域", Category = 1)]
    class SettingsAreaActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var key = this.AsyncDialog("Key", g => this.DialogValue("Select"));

            var Type = Utility.Parse(this.AsyncDialog("Type", g => this.DialogValue("Province")), LocationType.Province);

            var Parent = Utility.IntParse(this.AsyncDialog("Parent", g => this.DialogValue("0")), 0);

            var ValueId = Utility.IntParse(Web.UIDialog.AsyncDialog("Value", d =>
            {
                if (request.SendValues == null || request.SendValues.ContainsKey("start") == false)
                {
                    var buider = new UISectionBuilder(request.Model, request.Command, request.Arguments);
                    buider.CloseEvent("UI.Event");
                    this.Context.Send(buider.Builder(), true);
                }
                var send = new UMC.Web.WebMeta(request.Arguments.GetDictionary());
                UITitle uITItle = UITitle.Create();

                switch (Type)
                {
                    case Data.Entities.LocationType.Nation:
                        uITItle.Title = "选择国家";
                        break;
                    case Data.Entities.LocationType.City:
                        uITItle.Title = "选择城市";
                        break;
                    case Data.Entities.LocationType.Province:
                        uITItle.Title = "选择省份";
                        break;
                    case Data.Entities.LocationType.Region:
                        uITItle.Title = "选择区县";
                        break;
                }

                var sestion = UISection.Create(uITItle);
                var ui = sestion;
                if (Parent > 0)
                {


                    var cCode = DataFactory.Instance().Location(Parent);// ;// ?? new UMC.Data.Entities.Location { Id = 0 };

                    var title = "返回省份";

                    switch (Type)
                    {
                        case Data.Entities.LocationType.Region:
                            title = "返回城市";
                            break;
                    }

                    sestion.AddCell('\uf112', title, cCode.Name, UIClick.Query(new WebMeta().Put("Parent", cCode.ParentId).Put("Type", cCode.Type)));
                    ui = sestion.NewSection();
                }


                //entity.Where.Reset().And().Equal(new Location
                //{
                //    ParentId = Parent,
                //    Type = Type
                //})
                Data.Utility
                 .Each(DataFactory.Instance().Location(Parent, Type), dr =>
                  {
                      switch (dr.Type)
                      {
                          case LocationType.Region:
                              ui.AddCell(dr.Name, new Web.UIClick(new WebMeta(request.Arguments).Put(d, dr.Id)).Send(request.Model, request.Command));
                              break;
                          default:
                              ui.AddCell(dr.Name, Web.UIClick.Query(new WebMeta().Put("Type", dr.Type + 1).Put("Parent", dr.Id)));
                              break;

                      }


                  });
                response.Redirect(sestion);
                return this.DialogValue("none");

            }), 0);

            var region = DataFactory.Instance().Location(ValueId);// entity.Where.Reset().And().Equal(new Location { Id = ValueId }).Entities.Single();

            var city = DataFactory.Instance().Location(region.ParentId.Value);// entity.Where.Reset().And().Equal(new Location { Id = region.ParentId, Type = (region.Type - 1) }).Entities.Single();

            var province = DataFactory.Instance().Location(city.ParentId.Value);// entity.Where.Reset().And().Equal(new Location { Id = city.ParentId, Type = (city.Type - 1) }).Entities.Single();


            var area = string.Format("{0} {1} {2}", province.Name, city.Name, region.Name);

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new Web.ListItem { Text = area, Value = area }), true);



        }
    }
}