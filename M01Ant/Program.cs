using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiedTool;

namespace M01Ant
{
    class Program
    {
        private static readonly object LockFile = new object();  

        static void Main()
        {
            //Console.Write(Path.GetPathRoot(Path.GetFullPath(".")));
            //InvokeStringMethod("Logging", "test");
            //Mobile01Tool.Testit();
            //Logging("Mobile01 bot start");
            //ThriftTool.ThriftTool.
            //PraseQueue(5);
            //Logging("Mobile01 bot end");
            //PraseFourm();
            try
            {
                var client = ThriftTool.GetClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
            }
            
        }

        static List<string> LoadingFourmList()
        {
            var lineList = new List<string>();
            string line = null;
            var file =new StreamReader("forumlist.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length>10)
                {
                    if(line.Substring(0,1)!="#") lineList.Add(line);
                }
                
                line = null;
            }

            file.Close();
            return lineList;
        }

        static void PraseFourm()
        {
            Logging("Start Prasing Forum");
            var list = LoadingFourmList();
            foreach (var fourmUrl in list)
            {
                Logging("Prasing Forum : " + fourmUrl);
                Mobile01Tool.ProcessForum(fourmUrl);
                Thread.Sleep(Mobile01Tool.ChangeForum);
            }

        }

        static void PraseQueue(int limit)
        {
            Logging("Prasing Queue limit : " + limit);
            Mobile01Tool.ProcessQueue();
        }

        public static void Logging(string log)
        {
            //if(level!= (int)DebugLevel.Info)
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff ") + log);
            WriteFile(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + log);
        }

        //static void Logging(string log)
        //{
        //    Logging(log,(int)DebugLevel.Error);
        //}

        private static void WriteFile(string logtxt)
        {
            using (var fs = new FileStream(GetPath(@"logs\","Mobile01"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var log = new StreamWriter(fs))
                {
                    lock (LockFile)
                    {
                        log.WriteLine(logtxt);
                    }
                }
            }
        }

        private static string GetPath(string directory,string file)
        {
            var filename = file + DateTime.Now.ToString(" yyyy-MM-dd") + ".txt";
            directory = Path.GetPathRoot(Path.GetFullPath(".")) + directory;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory + filename;
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

    //public enum DebugLevel
    //{
    //    Error=0,
    //    Warning=1,
    //    Info=2
    //}
}
