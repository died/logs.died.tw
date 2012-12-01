using System;
using System.Collections.Generic;
using System.Text;
using Apache.Cassandra;
using Thrift.Protocol;
using Thrift.Transport;

namespace Mvc4.App_Data
{
    public class ThriftTool
    {
        private static TTransport _transport;
        private static string _keySpace="default";

        #region Get
        public static Cassandra.Client GetClient()
        {
            if(_transport==null) _transport = new TFramedTransport(new TSocket("localhost", 9160));
            TProtocol frameProtocol = new TBinaryProtocol(_transport);
            var client = new Cassandra.Client(frameProtocol);
            if(!_transport.IsOpen) _transport.Open();
            client.set_keyspace(_keySpace);
            return client;
        }

        public static List<KeySlice> GetAllFromCF(string cf, int count)
        {
            return GetClient().get_range_slices(GetParent(cf), GetPredicate(count), GetKeyRange(count), ConsistencyLevel.ONE);
        }

        public static bool CheckExist(string key, string cf)
        {
            var count = GetClient().get_count(ToByte(key), GetParent(cf), GetPredicate(5), ConsistencyLevel.ONE);
            return (count > 0);
        }

        public static CqlResult GetByCql(string query)
        {
            return GetClient().execute_cql_query(ToByte(query), Compression.NONE);
        }

        #endregion

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

        public static void AddClass<T>(T target,string key,string cf)
        {
            foreach (T element in (IEnumerable<T>) target)
            {
                
            }
        }

        public static void CounterAdd(string key,string cf,string name,int incre)
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
            return BitConverter.ToInt32(byt, 0);
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

        public static CounterColumn NewCounterColumn(string key, int value)
        {
            return new CounterColumn
            {
                Name = ToByte(key),
                Value = value
            };
        }

        public static SlicePredicate GetPredicate (int count)
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

        public static SlicePredicate GetPredicate(string start,string end, int count)
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

        public static KeyRange GetKeyRange(int count)
        {
            return new KeyRange
            {
                Count = count,
                Start_key = new byte[0],
                End_key = new byte[0]
            };
        }

        public static KeyRange GetKeyRange(string startKey,string endKey,int count)
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
            if(_transport.IsOpen) _transport.Close();
            _transport.Dispose();
        }

        #endregion 
    }
}