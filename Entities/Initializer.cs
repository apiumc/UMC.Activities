using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Data.Sql;
using UMC.Net;

namespace UMC.Activities.Entities
{
    [Web.Mapping]
    public class Initializer : UMC.Data.Entities.Initializer
    {
        public Initializer()
        {
            this.Setup<Design_Config>(new Design_Config { Id = Guid.Empty }, new Design_Config { Value = String.Empty });
            this.Setup<Design_Item>(new Design_Item { Id = Guid.Empty }, new Design_Item
            {
                Style = String.Empty,
                Data = String.Empty,
                Click = String.Empty
            });
        }
        public override string ResourceJS => "UMC.Activities.Resources.UMC.js";

        //public override void Setup(IDictionary hash)
        //{
        //    base.Setup(hash);
        //    var locationEntity = Data.Database.Instance().ObjectEntity<UMC.Data.Entities.Location>();

        //    if (locationEntity.Count() == 0)
        //    {
        //        Reflection.Start(() =>
        //         {
        //             var path = "UMC.Activities.Resources.Location.csv";

        //             var reader = new System.IO.StreamReader(typeof(Initializer).Assembly.GetManifestResourceStream(path), Encoding.UTF8);


        //             try
        //             {

        //                 CSV.EachRow(reader, data =>
        //                 {
        //                     if (data.Length > 3)
        //                     {
        //                         for (var i = 0; i < 4; i++)
        //                         {
        //                             if (String.IsNullOrEmpty(data[i]))
        //                             {
        //                                 return;
        //                             }
        //                         }
        //                         var s = new Location();
        //                         s.Id = Utility.IntParse(data[0], 0);
        //                         s.Type = Utility.Parse(data[1], LocationType.Nation);
        //                         s.Name = data[2]; ;
        //                         s.ParentId = Utility.IntParse(data[3], 0);
        //                         if (data.Length > 4)
        //                         {
        //                             s.ZipCode = data[4];
        //                         }
        //                         locationEntity.Insert(s);
        //                     }

        //                 });

        //             }
        //             catch//(Exception ex)
        //             {

        //             }
        //             finally
        //             {
        //                 reader.Close();
        //             }

        //         });
        //    }
        //}
    }
}
