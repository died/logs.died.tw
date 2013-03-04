using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apache.Cassandra;
using DiedTool;

namespace M01Ant
{
    class Program
    {
  

        static void Main()
        {
            //Console.Write(Path.GetPathRoot(Path.GetFullPath(".")));
            //InvokeStringMethod("Logging", "test");
            //Mobile01Tool.Testit();
            //Logging("Mobile01 bot start");
            //ThriftTool.ThriftTool.
            //PraseQueue(5000);
            PraseNnN();
            //Logging("Mobile01 bot end");
            //PraseFourm();


        }

        static List<string> LoadingFourmList()
        {
            var lineList = new List<string>();
            string line;
            var file =new StreamReader("forumlist.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length <= 10) continue;
                if(line.Substring(0,1)!="#") lineList.Add(line);
            }

            file.Close();
            return lineList;
        }

        static void PraseFourm()
        {
            Utility.Logging("Start Prasing Forum", Utility.DebugLevel.Info);
            var list = LoadingFourmList();
            foreach (var fourmUrl in list)
            {
                Utility.Logging("Prasing Forum : " + fourmUrl, Utility.DebugLevel.Info);
                Mobile01Tool.ProcessForum(fourmUrl);
                Thread.Sleep(Mobile01Tool.ChangeForum);
            }
        }

        static void PraseQueue(int limit)
        {
            Utility.Logging("Prasing Queue limit : " + limit,Utility.DebugLevel.Info);
            Mobile01Tool.ProcessQueue(limit);
        }


        static void PraseNnN()
        {
            var count = 0;
            //Utility.Logging("Calculating N vs N from: " + count, Utility.DebugLevel.Info);
            var re = ThriftTool.GetAllFromCF("M01UserRelaction", 120000);
            foreach(var node in re)
            {
                if (count % 1000 == 0) Utility.Logging("Calculating N vs N from: " + count, Utility.DebugLevel.Info);
                foreach (var keySlice in node.Columns)
                {
                    if (node.Key != keySlice.Counter_column.Name)
                    {
                        Check2NodeValue(ThriftTool.ToString(node.Key), ThriftTool.ToString(keySlice.Counter_column.Name), keySlice.Counter_column.Value);
                    }
                }
                count++;
            }
            Utility.Logging("total count: " + count, Utility.DebugLevel.Info);
        }

        static void Check2NodeValue(string n1,string n2,long val)
        {
            if (n1 == n2) return;
            var secondVal = ThriftTool.GetSingleCounter(ThriftTool.ToByte(n2), "M01UserRelaction", n1);

            //save N1>N2
            if (Int32.Parse(n2) > Int32.Parse(n1)) Utility.Swap(ref n1, ref n2);
            ThriftTool.CounterAdd(n1 + "_" + n2, "M01UserRelactionN2", "sum", val + secondVal);
        }

        static long Get2NodeValue(int n1,int n2)
        {
            if (n2 > n1) Utility.Swap(ref n1, ref n2);
            return ThriftTool.GetSingleCounter(ThriftTool.ToByte(n1 + "_" + n2), "M01UserRelactionN2", "sum");
        }

        /// <summary>
        /// Dynamic Calling Method by String
        /// </summary>
        /// <param name="methodName">Method Name </param>
        /// <param name="stringParam">Parameter</param>
        public static void InvokeStringMethod(string methodName, string stringParam)
        {
            Type calledType = typeof(Program);

            calledType.InvokeMember(
                        methodName,
                        BindingFlags.InvokeMethod | BindingFlags.Public |
                            BindingFlags.Static,
                        null,
                        null,
                        new Object[] { stringParam });

        }  

    }


}
