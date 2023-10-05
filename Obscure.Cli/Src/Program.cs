using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Emit;
using Obscure.Cli;
using Obscure.Cli.Static;
using Obscure.Core;
using Obscure.Core.Utility;

namespace Obscure.Cli
{
    internal class Program
    {
        static string Graphic = Config.IsWindows11 ? @"
  ⡀⢀⣴⣾⠿⢿⣶⣄⡀⡀⡀⣿⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⢸⣿⡄⡀⡀⡀⡀⣿⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⢀
  ⢀⣿⠁⡀⡀⡀⡀⢻⣧⡀⡀⣿⡀⢀⣀⡀⡀⡀⡀⡀⣀⣀⣀⡀⡀⡀⡀⢀⣀⣀⡀⡀⣀⡀⡀⡀⡀⣀⡀⡀⣀⡀⡀⣀⡀⡀⡀⡀⣀⣀⡀⡀⡀⡀⡀⡀⡀⢸⡏⣿⡄⡀⡀⡀⣿⡀⡀⡀⡀⣀⣀⡀⡀⡀⢀⣿⣀⣀
  ⢸⡏⡀⡀⡀⡀⡀⡀⣿⡀⡀⣿⡟⠉⠉⠻⣷⡀⡀⣿⠋⠉⠉⠃⡀⢀⣿⠛⠉⠙⡀⡀⣿⡀⡀⡀⡀⣿⡀⡀⣿⣷⠟⠉⠁⡀⣾⠛⠉⠙⣿⡄⡀⡀⡀⡀⡀⢸⡇⡀⣿⣄⡀⡀⣿⡀⡀⣰⡟⠉⠉⢻⣦⡀⠉⣿⠉⠉
  ⢸⣇⡀⡀⡀⡀⡀⡀⣿⡀⡀⣿⡀⡀⡀⡀⣿⡀⡀⠻⣷⣤⣀⡀⡀⣾⡇⡀⡀⡀⡀⡀⣿⡀⡀⡀⡀⣿⡀⡀⣿⡇⡀⡀⡀⢰⣿⣶⣶⣶⣾⡧⡀⡀⡀⡀⡀⢸⡇⡀⡀⢿⣆⡀⣿⡀⡀⣿⣶⣶⣶⣶⣿⡀⡀⣿
  ⠈⣿⡀⡀⡀⡀⡀⣼⠟⡀⡀⣿⡀⡀⡀⡀⣿⡀⡀⡀⡀⡀⠙⣿⡀⢻⣧⡀⡀⡀⡀⡀⣿⡀⡀⡀⢠⣿⡀⡀⣿⡇⡀⡀⡀⠈⣿⡀⡀⡀⡀⡀⡀⡀⡀⡀⡀⢸⡇⡀⡀⡀⢿⣆⣿⡀⡀⣿⡄⡀⡀⡀⡀⡀⡀⣿
  ⡀⠈⠻⢿⣶⣶⠿⠋⡀⡀⡀⣿⠻⣶⣶⡿⠋⡀⡀⢿⣶⣶⡾⠋⡀⡀⠻⢷⣶⣾⠃⡀⠙⢿⣶⡶⠋⣿⡀⡀⣿⡇⡀⡀⡀⡀⠙⠿⣶⣶⡾⠃⡀⢸⣿⡀⡀⢸⡇⡀⡀⡀⡀⢿⣿⡀⡀⠈⠻⣷⣶⣶⠟⡀⡀⠹⣿⣶⠆" : @"
    ____  _                               _   _      _   
   / __ \| |                             | \ | |    | |  
  | |  | | |__  ___  ___ _   _ _ __ ___  |  \| | ___| |_ 
  | |  | | '_ \/ __|/ __| | | | '__/ _ \ | . ` |/ _ \ __|
  | |__| | |_) \__ \ (__| |_| | | |  __/_| |\  |  __/ |_ 
   \____/|_.__/|___/\___|\__,_|_|  \___(_)_| \_|\___|\__|";

        static Obfuscator Obfuscator = new Obfuscator
        {
            Settings = new ObfuscationSettings
            {
                FileName = "Obscure-Obfuscated.exe",
                Overwrite = true,
            }
        };
        
        static void Main(string[] Parameters)
        {
            Console.Title = $"Obscure.Cli | Version {Config.Version} ({(Config.IsWindows11 ? "Windows 11" : "Windows 10")})";
            Console.SetWindowSize(80, 20);

            Console.OutputEncoding = Encoding.Unicode;
            TextWriter.Print(ConsoleColor.Blue, Graphic + "\n\n");

            if (Parameters.Length is 0)
                TextWriter.Error("Failed To Set Module Path, Drag And Application Onto Obscure.Cli To Set Path.");

            string Path = Parameters[0];
            string BinaryName = Path.Split('\\').Last();

            ObfuscationContext Context = new ObfuscationContext(Path);

            if (BinaryName == Obfuscator.Settings.FileName)
                TextWriter.Error("Binary Is Already Obfuscated.");

            TextWriter.Print(ConsoleColor.White, $"Queued Binary : \"{BinaryName}\"\n");

            TextWriter.Print(ConsoleColor.White, "Applying .Net Protections...\n\n");
            Obfuscator.Obfuscate(Context);
            
            TextWriter.Print(ConsoleColor.White, "\n  Protection Successful, Press Any Key To Exit...");

            Console.ReadKey(true);
        }
    }
}