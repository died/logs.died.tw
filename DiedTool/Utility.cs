using System;
using System.IO;

namespace DiedTool
{
    public class Utility
    {
        private static readonly object LockFile = new object();

        public static void Logging(string log,DebugLevel level)
        {
            var prefix = string.Empty;
            var shortTime = DateTime.Now.ToString("HH:mm:ss.fff ");
            var longTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff ");
            var text = shortTime + log;
            //if(level!= DebugLevel.Info)
            switch (level)
            {
                case DebugLevel.Error:
                    prefix = "[Error]";
                    ColoredConsoleWrite(ConsoleColor.Red, prefix + text);
                    break;
                case DebugLevel.Warning:
                    prefix = "[Warning]";
                    ColoredConsoleWrite(ConsoleColor.Yellow, prefix + text);
                    break;
                default:
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ffff ") + log);
                    break;

            }
            
            WriteFile(prefix + longTime + log);
        }

        public static void Logging(string log)
        {
            Logging(log, DebugLevel.Error);
        }

        private static void WriteFile(string logtxt)
        {
            using (var fs = new FileStream(GetPath(@"logs\", "Mobile01"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
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

        private static string GetPath(string directory, string file)
        {
            var filename = file + DateTime.Now.ToString(" yyyy-MM-dd") + ".txt";
            directory = Path.GetPathRoot(Path.GetFullPath(".")) + directory;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory + filename;
        }

        public static void ColoredConsoleWrite(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        public enum DebugLevel
        {
            Error = 0,
            Warning = 1,
            Info = 2
        }
    }
}
