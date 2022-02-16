using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using UMC.Net;
using UMC.Activities.Entities;
using UMC.Data;

namespace UMC.Activities
{
    static class Mather
    {

        public static int MAX<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            if (source.Count() == 0)
            {
                return 0;
            }
            return source.Max(selector);
        }
    }

    public class DataFactory
    {
        public static DataFactory Instance()
        {
            return _Instance;
        }
        static DataFactory _Instance = new DataFactory();
        public static void Instance(DataFactory dataFactory)
        {
            _Instance = dataFactory;
        }

        public virtual Design_Config[] DesignConfig(String groupBy)
        {


            return Database.Instance().ObjectEntity<Design_Config>()
                         .Where.And().Equal(new Design_Config
                         {
                             GroupBy = groupBy,
                         }).Entities.Query();

        }
        public virtual Data.Entities.Location[] Location(int parent, Data.Entities.LocationType type)
        {
            var webr = new Uri($"https://api.365lu.cn/Location?ParentId={parent}&Type={type}").WebRequest(); 

            webr.Headers.Add("umc-client-pfm", "sync");
            webr.Headers.Add("umc-sync-type", "array");
            var httpResponse = webr.Get();//.ReadAsString();
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return UMC.Data.JSON.Deserialize<Data.Entities.Location[]>(httpResponse.ReadAsString());
                
            }
            return new Data.Entities.Location[0]; 

        }
        public virtual Data.Entities.Location Location(int code)
        {
            var webr = new Uri($"https://api.365lu.cn/Location?Id={code}").WebRequest();

            webr.Headers.Add("umc-client-pfm", "sync");
            webr.Headers.Add("umc-sync-type", "single");
            var httpResponse = webr.Get();//.ReadAsString();
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return UMC.Data.JSON.Deserialize<Data.Entities.Location>(httpResponse.ReadAsString());

            }
            return null; 

        }
        public virtual Design_Item[] DesignItems(Guid designId, params Guid[] designIds)
        {
            return Database.Instance().ObjectEntity<Design_Item>()
                         .Where.And().In(new Design_Item
                         {
                             design_id = designId,
                         }, designIds).Entities.Order.Asc(new Design_Item { Seq = 0 }).Entities.Query();

        }
        public virtual void Delete(Design_Item item)
        {
            if (item.Id.HasValue)
            {
                Database.Instance().ObjectEntity<Design_Item>()
               .Where.And().Equal(new Design_Item
               {
                   Id = item.Id.Value,
               }).Entities.Delete();
            }
        }
        public virtual void Put(Design_Item item)
        {
            if (item.Id.HasValue)
            {
                Database.Instance().ObjectEntity<Design_Item>()
               .Where.And().Equal(new Design_Item
               {
                   Id = item.Id.Value,
               }).Entities.IFF(e => e.Update(item) == 0, e => e.Insert(item));
            }
        }

        public virtual void Delete(Design_Config item)
        {
            if (item.Id.HasValue)
            {
                Database.Instance().ObjectEntity<Design_Config>()
               .Where.And().Equal(new Design_Config
               {
                   Id = item.Id.Value,
               }).Entities.Delete();
            }
        }
        public virtual void Put(Design_Config item)
        {
            if (item.Id.HasValue)
            {
                Database.Instance().ObjectEntity<Design_Config>()
               .Where.And().Equal(new Design_Config
               {
                   Id = item.Id.Value,
               }).Entities.IFF(e => e.Update(item) == 0, e => e.Insert(item));
            }
        }

        public virtual Design_Item[] DesignItems(Guid designId, Guid forid)
        {


            return Database.Instance().ObjectEntity<Design_Item>()
                         .Where.And().Equal(new Design_Item
                         {
                             design_id = designId,
                             for_id = forid,
                         }).Entities.Order.Asc(new Design_Item { Seq = 0 }).Entities.Query();

        }

        public virtual Design_Item DesignItem(Guid itemid)
        {


            return Database.Instance().ObjectEntity<Design_Item>()
                         .Where.And().Equal(new Design_Item
                         {
                             Id = itemid,
                         }).Entities.Single();

        }
        public virtual Design_Config DesignConfig(Guid root)
        {
            return Database.Instance().ObjectEntity<Design_Config>()
                         .Where.And().Equal(new Design_Config
                         {
                             Id = root
                         }).Entities.Single();

        }
    }
}
