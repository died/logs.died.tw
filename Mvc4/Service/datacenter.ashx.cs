using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Apache.Cassandra;
using Newtonsoft.Json;
using Thrift.Transport;
using Mvc4.App_Data;

namespace Mvc4.Service
{
    /// <summary>
    ///datacenter 的摘要描述
    /// </summary>
    public class Datacenter : IHttpHandler
    {

        private TTransport _transport;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            var mode = context.Request.QueryString["m"];
            var date = context.Request.QueryString["d"];
            var type = context.Request.QueryString["t"];
            var str = string.Empty;
            if(mode=="d")   //mode=get one day
                str = JsonConvert.SerializeObject(BahaGetOneDay(date, GetColumnFamily(type)));
            context.Response.Write(str);

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string GetColumnFamily(string type)
        {
            string result;
            int i;
            int.TryParse(type, out i);
            switch(i)
            {
                case 1:
                    result = "BahamutAll";
                    break;
                case 2:
                    result = "BahamutOnline";
                    break;
                case 3:
                    result = "BahamutDays";
                    break;
                default:
                    result = "BahamutAll";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 取得Baha單日資料
        /// </summary>
        /// <param name="key">日期</param>
        /// <param name="columnFamily">CF name (BahamutOnline,BahamutAll)</param>
        /// <returns></returns>
        public IOrderedEnumerable<RankList> BahaGetOneDay(string key, string columnFamily)
        {
            var rank = new List<RankList>();

            var client = ThriftTool.GetClient("default",ref _transport);
            var parent = new ColumnParent { Column_family = columnFamily };
            var predicate = new SlicePredicate
            {
                Slice_range = new SliceRange
                {
                    Start = new byte[0],
                    Finish = new byte[0],
                    Count = 100,
                    Reversed = false
                }
            };
            var lb = new List<byte[]> { ThriftTool.ToByte(key) };

            Dictionary<byte[], List<ColumnOrSuperColumn>> results = client.multiget_slice(lb, parent, predicate, ConsistencyLevel.ONE);

            if (results.Count > 0) //if have result
            {
                foreach (var result in results)
                {
                    foreach (var scol in result.Value)
                    {
                        if (scol.GetType() == typeof(ColumnOrSuperColumn))
                        {
                            var rl = new RankList();
                            foreach (var col in scol.Super_column.Columns)
                            {
                                string name = ThriftTool.ToString(col.Name);
                                if (name == "Title") rl.Title = ThriftTool.ToString(col.Value);
                                if (name == "Rank") rl.Rank = int.Parse(ThriftTool.ToString(col.Value));
                                if (name == "Article") rl.Article = ThriftTool.ToString(col.Value);
                                if (name == "Link") rl.Link = ThriftTool.ToString(col.Value);
                                if (name == "Popular") rl.Popular = ThriftTool.ToString(col.Value);
                                if (name == "Date") rl.Date = DateTime.Parse(ThriftTool.ToString(col.Value));
                                if (name == "Change") rl.Change = ThriftTool.ToString(col.Value);
                            }
                            rank.Add(rl);
                        }
                    }
                }
            }

            ThriftTool.TransportClose(ref _transport);
            var ranks = from n in rank orderby n.Rank select n;

            return ranks;
        }
    }
}