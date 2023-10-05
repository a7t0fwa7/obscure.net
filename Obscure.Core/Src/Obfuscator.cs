using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet.Writer;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Obscure.Core.Engine;
using Obscure.Core.Engine.Protections;
using System.Diagnostics;
using Obscure.Core.Utility;

namespace Obscure.Core
{
    public class ObfuscationContext
    {
        public readonly ModuleDefMD Module;

        public ObfuscationContext(string Path) => Module = ModuleDefMD.Load(Path);
    }

    public class ObfuscationSettings
    {
        public string FileName;
        public bool Overwrite;
    }
    
    public class Obfuscator
    {
        public ObfuscationSettings Settings;

        private Protection[] Protections = new Protection[]
        {
            new NameEncode(),
            new JunkEncode(),
            new StringEncode(),
            new IntEncode(),
            new ProxyEncode(),
        };
        
        public void Obfuscate(ObfuscationContext Context)
        {
            if (Context is null || Settings is null)
                return;

            foreach (Protection Protection in Protections)
            {
                Console.WriteLine($"  -Executing Protection : {Protection.Name}");
                Protection.Run(Context);
            }

            TypeRef AttributeReference = Context.Module.CorLibTypes.GetTypeRef("System", "Attribute");
            TypeDefUser Type = new TypeDefUser(string.Empty, "ObscureAttribute", AttributeReference);
            Context.Module.Types.Add(Type);

            MethodDefUser Method = new MethodDefUser(
                ".ctor",
                MethodSig.CreateInstance(Context.Module.CorLibTypes.Void, Context.Module.CorLibTypes.String),
                MethodImplAttributes.Managed,
                MethodAttributes.HideBySig
                | MethodAttributes.Public
                | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName)
            {
                Body = new CilBody(),
            };

            Method.Body.MaxStack = 1;
            Method.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
            Method.Body.Instructions.Add(OpCodes.Call.ToInstruction(
                new MemberRefUser(
                    Context.Module,
                    ".ctor",
                    MethodSig.CreateInstance(Context.Module.CorLibTypes.Void), AttributeReference)));

            Method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            Type.Methods.Add(Method);

            CustomAttribute Attribute = new CustomAttribute(Method);
            Attribute.ConstructorArguments.Add(new CAArgument(Context.Module.CorLibTypes.String, "Protected by Obscure.Net"));
            Context.Module.CustomAttributes.Add(Attribute);
            
            Context.Module.Write($".\\{Settings.FileName}", new ModuleWriterOptions(Context.Module)
            {
                Logger = DummyLogger.NoThrowInstance
            });
        }
    }
}
