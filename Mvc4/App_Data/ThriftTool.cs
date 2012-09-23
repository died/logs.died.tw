using System;
using System.Text;
using Apache.Cassandra;
using Thrift.Protocol;
using Thrift.Transport;

namespace Mvc4.App_Data
{
    public class ThriftTool 
    {
        #region ThriftTool
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

        public void AddColumn(string key, string cf, string name, string value, Cassandra.Client client)
        {
            var cp = new ColumnParent { Column_family = cf };
            client.insert(ToByte(key), cp, NewColumn(name, value), ConsistencyLevel.ONE);
        }

        public void AddColumn(string key, string cf, string name, int value, Cassandra.Client client)
        {
            var cp = new ColumnParent { Column_family = cf };
            client.insert(ToByte(key), cp, NewColumn(name, value), ConsistencyLevel.ONE);
        }

        public static void TransportClose(ref TTransport transport)
        {
            transport.Flush();
            transport.Close();
        }

        #endregion 
    }
}