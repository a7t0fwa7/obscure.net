using System.Collections.Generic;

using dnlib.DotNet;

namespace Obscure.Core.Injection
{
    public class InjectContext
    {
        public readonly Dictionary<IDnlibDef, IDnlibDef> Map = new Dictionary<IDnlibDef, IDnlibDef>();

        public readonly ModuleDef OriginModule;

        public readonly ModuleDef TargetModule;

        public InjectContext(ModuleDef Module, ModuleDef Target)
        {
            OriginModule = Module;
            TargetModule = Target;
            Importer = new Importer(Target, ImporterOptions.TryToUseTypeDefs);
        }

        public Importer Importer { get; }
    }
}
