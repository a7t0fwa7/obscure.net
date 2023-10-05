using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscure.Cli.Static
{
    public static class Config
    {
        public static string Version = "1.0.1";
        public static bool IsWindows11 = Environment.OSVersion.Version.Build >= 20000 ? true : false;
    }
}
