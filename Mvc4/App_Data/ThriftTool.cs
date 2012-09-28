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
        #region Get
        public static Cassandra.Client GetClient(string keyspace, ref TTransport transport)
        {
            TTransport frameTransport = new TFramedTransport(new TSocket("localhost", 9160));
            TProtocol frameProtocol = new TBinaryProtocol(frameTransport);
            var client = new Cassandra.Client(frameProtocol, frameProtocol);
            frameTransport.Open();
            client.set_keyspace(keyspace);
            transport = frameTransport;
            return client;
        }

        public static List<KeySlice> GetAllFromCF(string cf, int count, Cassandra.Client client)
        {
            return client.get_range_slices(GetParent(cf), GetPredicate(count), GetKeyRange(count), ConsistencyLevel.ONE);
        }
        #endregion

        #region Set
        public void AddColumn(string key, string cf, string name, string value, Cassandra.Client client)
        {
            client.insert(ToByte(key), GetParent(cf), NewColumn(name, value), ConsistencyLevel.ONE);
        }

        public void AddColumn(string key, string cf, string name, int value, Cassandra.Client client)
        {
            client.insert(ToByte(key), GetParent(cf), NewColumn(name, value), ConsistencyLevel.ONE);
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

        public Column NewColumn(string key, string value)
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

        public Column NewColumn(string key, int value)
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
        /// <param name="transport">Exists Connect</param>
        public static void TransportClose(ref TTransport transport)
        {
            transport.Flush();
            transport.Close();
        }

        #endregion 
    }
}