using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscure.Core.Engine
{
    public abstract class Protection
    {
        public string Name { get; set; }

        public abstract void Run(ObfuscationContext Context);
    }
}