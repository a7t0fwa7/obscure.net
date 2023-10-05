using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Obscure.Core.Utility;

namespace Obscure.Core.Engine.Protections
{
    public class JunkEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            foreach (TypeDef Type in Context.Module.Types)
            {
                foreach (MethodDef Method in Type.Methods.ToArray())
                {
                    MethodDef Strings = CreateJunkMethod(Utils.RandomString(Utils.Next(50, 70)), Method);
                    MethodDef Integers = CreateJunkMethod(Utils.Next(11111, 999999999), Method);

                    Type.Methods.Add(Strings);
                    Type.Methods.Add(Integers);
                }
            }
        }
        
        public static MethodDef CreateJunkMethod(object Input, MethodDef Source)
        {
            CorLibTypeSig Core = null;

            if (Input is int)
                Core = Source.Module.CorLibTypes.Int32;
            else if (Input is string)
                Core = Source.Module.CorLibTypes.String;
            else if (Input is float)
                Core = Source.Module.CorLibTypes.Single;

            MethodDef JunkMethod = new MethodDefUser(Utils.RandomString(50),
                    MethodSig.CreateStatic(Core),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Body = new CilBody()
            };

            if (Input is int)
                JunkMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, (int)Input));
            else if (Input is string)
                JunkMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, (string)Input));
            else if (Input is float)
                JunkMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (double)Input));

            JunkMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));

            return JunkMethod;
        }
        
        public JunkEncode() => Name = "Junk Code";
    }
}
