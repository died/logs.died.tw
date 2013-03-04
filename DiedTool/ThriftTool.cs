using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Cassandra;
using Thrift.Protocol;
using Thrift.Transport;

namespace DiedTool
{
    public class ThriftTool
    {
        private static TTransport _transport;
        //private static Cassandra.Client _client;
        private static string _keySpace = "default";
        private static bool _setKeySpace = false;

        #region Get
        public static Cassandra.Client GetClient()
        {
            //if (_client == null)
            //{
                if (_transport == null) _transport = new TFramedTransport(new TSocket("localhost", 9160));
                //TProtocol frameProtocol = new TBinaryProtocol(_transport);
                var client = new Cassandra.Client(new TBinaryProtocol(_transport));
                if(!_transport.IsOpen)
                {
                    try
                    {
                        _transport.Open();
                    }catch(Exception)
                    {
                    }

                }

                    
                if (!_setKeySpace)
                {
                    client.set_keyspace(_keySpace);
                    _setKeySpace = true;
                }
                return client;
            //}
            //return _client;
        }

        //public ThriftTool()
        //{
        //    if (_client == null)
        //    {
        //        _client = GetClient();
        //    }
            
        //}

        //~ThriftTool()
        //{
        //    _transport.Close();
        //    _transport.Dispose();
        //    _client = null;
        //}

        /// <summary>
        /// 取得某個Column Family內的指定數量列資料
        /// </summary>
        /// <param name="cf">Column Family</param>
        /// <param name="count">數量</param>
        /// <returns></returns>
        public static List<KeySlice> GetAllFromCF(string cf, int count)
        {
            return GetClient().get_range_slices(GetParent(cf), GetPredicate(count), GetKeyRange(count), ConsistencyLevel.ONE);
        }

        public static Dictionary<byte[], List<ColumnOrSuperColumn>> GetMultiSlice(List<byte[]> lb, string columnFamily,int count)
        {
            return GetClient().multiget_slice(lb, GetParent(columnFamily), GetPredicate(count), ConsistencyLevel.ONE);
        }

        public static List<ColumnOrSuperColumn> GetSingleKey(byte[] key, string columnFamily, int count)
        {
            //Utility.Logging("key="+ToString(key));
            
            return GetClient().get_slice(key, GetParent(columnFamily), GetPredicate(count), ConsistencyLevel.ONE);
        }

        public static long GetSingleCounter(byte[] key,string columnFamily,string count)
        {
            try
            {
                return GetClient().get(key, GetColumnPath(columnFamily, count), ConsistencyLevel.ONE).Counter_column.Value;
            }
            catch (NotFoundException)
            {
                return 0;
            }
        }


        /// <summary>
        /// Check key exist or not
        /// 確認某個key值是否存在
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="cf">Column Family</param>
        /// <returns></returns>
        public static bool CheckExist(string key, string cf)
        {
            var count = GetClient().get_count(ToByte(key), GetParent(cf), GetPredicate(5), ConsistencyLevel.ONE);
            return (count > 0);
        }

        /// <summary>
        /// Using Cassandra Query Language to query and get result
        /// 回傳CQL查詢結果
        /// </summary>
        /// <param name="query">Query String</param>
        /// <returns></returns>
        public static CqlResult GetByCql(string query)
        {
            return GetClient().execute_cql_query(ToByte(query), Compression.NONE);
        }

        #endregion

        /// <summary>
        /// 設定KeySpace所在
        /// </summary>
        /// <param name="keyspace">KeySpace</param>
        public static void SetKeySpace(string keyspace)
        {
            _keySpace = keyspace;
        }

        #region Set
        public static void AddColumn(string key, string cf, string name, string value)
        {
            GetClient().insert(ToByte(key), GetParent(cf), NewColumn(name, value), ConsistencyLevel.ONE);
        }

        public static void AddColumn(string key, string cf, string name, int value)
        {
            GetClient().insert(ToByte(key), GetParent(cf), NewColumn(name, value), ConsistencyLevel.ONE);

        }

        //public static void AddClass<T>(T target, string key, string cf)
        //{
        //    foreach (T element in (IEnumerable<T>)target)
        //    {

        //    }
        //}

        public static void CounterAdd(string key, string cf, string name, long incre)
        {
            GetClient().add(ToByte(key), GetParent(cf), NewCounterColumn(name, incre), ConsistencyLevel.ONE);
        }

        #endregion

        #region Tool
        /// <summary>
        /// Convert string to Byte[]
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>byte[]</returns>
        public static byte[] ToByte(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Convert int to Byte[]
        /// </summary>
        /// <param name="i">int</param>
        /// <returns>byte[]</returns>
        public static byte[] ToByte(int i)
        {
            return BitConverter.GetBytes(i);
        }

        public static string ToString(byte[] byt)
        {
            return Encoding.UTF8.GetString(byt);
        }

        public static int ToInt(byte[] byt)
        {
            //if (BitConverter.IsLittleEndian) Array.Reverse(byt);
            return BitConverter.ToInt32(byt, 0);
        }

        public static long ToLong(byte[] byt)
        {
            //if (BitConverter.IsLittleEndian) Array.Reverse(byt);
            return BitConverter.ToUInt32(byt, 0);
        }

        public static Column NewColumn(string key, string value)
        {
            return new Column
            {
                Name = ToByte(key),
                Value = ToByte(value),
                Timestamp =
                    Convert.ToInt64(
                        DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds)
            };
        }

        public static Column NewColumn(string key, int value)
        {
            return new Column
            {
                Name = ToByte(key),
                Value = ToByte(value),
                Timestamp =
                    Convert.ToInt64(
                        DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds)
            };
        }

        public static CounterColumn NewCounterColumn(string key, long value)
        {
            return new CounterColumn
            {
                Name = ToByte(key),
                Value = value
            };
        }

        public static SlicePredicate GetPredicate(int count)
        {
            return new SlicePredicate
            {
                Slice_range = new SliceRange
                {
                    Start = new byte[0],
                    Finish = new byte[0],
                    Count = count,
                    Reversed = false
                }
            };
        }

        public static SlicePredicate GetPredicate(string start, string end, int count)
        {
            return new SlicePredicate
            {
                Slice_range = new SliceRange
                {
                    Start = ToByte(start),
                    Finish = ToByte(end),
                    Count = count,
                    Reversed = false
                }
            };
        }

        public static ColumnPath GetColumnPath(string cf,string counter)
        {
            return new ColumnPath
                       {
                           Column_family = cf,
                           Column = ToByte(counter)
                       };
        }

        public static KeyRange GetKeyRange(int count)
        {
            return new KeyRange
            {
                Count = count,
                Start_key = new byte[0],
                End_key = new byte[0]
            };
        }

        public static KeyRange GetKeyRange(string startKey, string endKey, int count)
        {
            return new KeyRange
            {
                Count = count,
                Start_key = ToByte(startKey),
                End_key = ToByte(endKey)
            };
        }

        public static ColumnParent GetParent(string cf)
        {
            return new ColumnParent { Column_family = cf };
        }

        /// <summary>
        /// Close Connection
        /// </summary>
        public static void TransportClose()
        {
            if (_transport == null) return;
            if (_transport.IsOpen) _transport.Close();
            _transport.Dispose();
            _transport = null;
        }

        #endregion
    }
}
