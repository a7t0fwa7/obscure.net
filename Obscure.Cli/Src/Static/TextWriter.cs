using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscure.Cli
{
    public static class TextWriter
    {
        public static void Print(ConsoleColor Color, params string[] Content)
        {
            string String = string.Empty;
            foreach (string Strings in Content)
                String += Strings;

            Console.ForegroundColor = Color;
            Console.Write("  " + String);
        }

        public static void Error(string Reason)
        {
            Print(ConsoleColor.Red, Reason);

            Console.ReadKey(true);
            Environment.Exit(0);
        }

        public static void Info(string Information) =>
            Print(ConsoleColor.Blue, Information);
    }
}
