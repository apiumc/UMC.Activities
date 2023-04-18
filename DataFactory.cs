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
        static DataFactory()
        {
            HotCache.Register<PageConfig>("Id").Register("AppKey", "GroupBy", "Id");
            HotCache.Register<PageItem>("Id").Register("AppKey", "design_id", "for_id", "Id");
        }
        public static DataFactory Instance()
        {
            return _Instance;
        }
        static DataFactory _Instance = new DataFactory();
        public static void Instance(DataFactory dataFactory)
        {
            _Instance = dataFactory;
        }

        public virtual PageConfig[] DesignConfig(Guid appKey, String groupBy)
        {
            int index;
            return HotCache.Find(new PageConfig { AppKey = appKey, GroupBy = groupBy }, 0, 100, out index)
                .OrderBy(r => r.Sequence ?? 0).ToArray();

        }
        public virtual Data.Entities.Location[] Location(int parent, Data.Entities.LocationType type)
        {
            var webr = new Uri($"https://api.apiumc.com/Location?ParentId={parent}&Type={type}").WebRequest();

            webr.Headers.Add("umc-client-pfm", "sync");
            webr.Headers.Add("umc-sync-type", "array");
            var httpResponse = webr.Get();
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return UMC.Data.JSON.Deserializes<Data.Entities.Location>(httpResponse.ReadAsString());

            }
            return new Data.Entities.Location[0];

        }
        public virtual Data.Entities.Location Location(int code)
        {
            var webr = new Uri($"https://api.apiumc.com/Location?Id={code}").WebRequest();

            webr.Headers.Add("umc-client-pfm", "sync");
            webr.Headers.Add("umc-sync-type", "single");
            var httpResponse = webr.Get();//.ReadAsString();
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return UMC.Data.JSON.Deserialize<Data.Entities.Location>(httpResponse.ReadAsString());

            }
            return null;

        }
        public virtual PageItem[] DesignItems(Guid appKey, Guid designId)
        {
            int index;
            var cache = HotCache.Cache<PageItem>();
            return cache.Find(new PageItem { AppKey = appKey, design_id = designId }, 0, out index);//, "design_id", designIds);
        }
        public virtual PageItem[] DesignItems(Guid appKey, Guid designId,  Guid[] designIds)
        {
            int index;
            var cache = HotCache.Cache<PageItem>();
            return cache.Find(new PageItem { AppKey = appKey, design_id = designId }, 0, out index, "design_id", designIds);

        }
        public virtual void Delete(PageItem item)
        {
            if (item.Id.HasValue)
            {
                HotCache.Cache<PageItem>().Delete(item);
            }
        }
        public virtual void Put(PageItem item)
        {
            if (item.Id.HasValue)
            {
                HotCache.Cache<PageItem>().Put(item);
            }
        }

        public virtual void Delete(PageConfig item)
        {
            if (item.Id.HasValue)
            {
                HotCache.Cache<PageConfig>().Delete(item);
            }
        }
        public virtual void Put(PageConfig item)
        {
            if (item.Id.HasValue)
            {
                HotCache.Cache<PageConfig>().Put(item);
            }
        }

        public virtual PageItem[] DesignItems(Guid appKey, Guid designId, Guid forid)
        {

            int index;
            var cache = HotCache.Cache<PageItem>();
            return cache.Find(new PageItem
            {
                AppKey = appKey,
                design_id = designId,
                for_id = forid,
            }, 0, out index);

        }

        public virtual PageItem DesignItem(Guid itemid)
        {


            return HotCache.Cache<PageItem>().Get(new PageItem
            {
                Id = itemid,
            });


        }
        public virtual PageConfig DesignConfig(Guid root)
        {
            return HotCache.Cache<PageConfig>().Get(new PageConfig
            {
                Id = root,
            });


        }
    }
}
