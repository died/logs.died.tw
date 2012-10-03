using System;
using System.Collections.Generic;
using System.Globalization;
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
            var game = context.Request.QueryString["g"];
            var startDate = context.Request.QueryString["s"];
            var endDate = context.Request.QueryString["e"];
            var str = string.Empty;
            switch (mode)
            {
                case "d":
                    str = JsonConvert.SerializeObject(BahaGetOneDay(date, GetColumnFamily(type)));
                    break;
                case "g":
                    str = JsonConvert.SerializeObject(BahaGetOneGame(game, startDate , endDate));
                    break;
            }
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

            var lb = new List<byte[]> { ThriftTool.ToByte(key) };

            Dictionary<byte[], List<ColumnOrSuperColumn>> results = client.multiget_slice(lb, ThriftTool.GetParent(columnFamily), ThriftTool.GetPredicate(100), ConsistencyLevel.ONE);

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

        public HighChart[] BahaGetOneGame(string game, string startDate, string endDate)
        {
            
            var gameList = game.Split(',');
            var chartList = new HighChart[gameList.Length];
            var client = ThriftTool.GetClient("default", ref _transport);

            for(var i=0;i<gameList.Length;i++)
            {
                var rank = new List<ScoreList>();
                CqlResult cqlResult = client.execute_cql_query(ThriftTool.ToByte("select * from BahamutGames where Title='" + gameList[i] + "'"), Compression.NONE);

                foreach (CqlRow t in cqlResult.Rows)
                {
                    var rl = new ScoreList();
                    foreach (var col in t.Columns)
                    {
                        var name = ThriftTool.ToString(col.Name);
                        switch (name)
                        {
                            case "Title":
                                rl.Title = ThriftTool.ToString(col.Value);
                                break;
                            case "Link":
                                rl.Link = ThriftTool.ToString(col.Value);
                                break;
                            case "Article":
                                rl.Article = ThriftTool.ToString(col.Value);
                                break;
                            case "Popular":
                                rl.Popular = ThriftTool.ToString(col.Value);
                                break;
                            case "Date":
                                rl.Date = ThriftTool.ToInt(col.Value);
                                break;
                        }
                    }
                    rank.Add(rl);
                }
                
                var ranks = from n in rank orderby n.Date select n;

                chartList[i] = ParseToHighChart(ranks);
            }
            ThriftTool.TransportClose(ref _transport);

            return chartList;
        }

        public HighChart ParseToHighChart(IOrderedEnumerable<ScoreList> source)
        {
            var hiChart = new HighChart();
            if(source.Any())
            {
                hiChart.Name = source.First().Title;
                hiChart.Data = source.Select(s => ParseDateAndNumber(s.Date, s.Popular)).ToList();
            }
            return hiChart;
        }

        public Popular ParseDateAndNumber(int date, string popular)
        {
            var t = date.ToString(CultureInfo.InvariantCulture);
            var sd = t.Substring(0, 4) + "/" + t.Substring(4, 2) + "/" + t.Substring(6,2);
            return new Popular { Date = DateTime.Parse(sd) ,Value = int.Parse(popular)};
        }
    }

    public class HighChart
    {
        public string Name { get; set; }
        public List<Popular> Data { get; set; }
    }

    public class Popular
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }
}