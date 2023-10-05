using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obscure.Core.Injection;
using Obscure.Core.Utility;

namespace Obscure.Core.Engine.Protections
{
    public static class StringDecoder
    {
        public static string Decrypt(string Input, int Min, int Key, int Hash, int Length, int Max)
        {
            if (Max > 78787878) ;
            if (Length > 485941) ;

            StringBuilder Builder = new StringBuilder();
            foreach (char Character in Input.ToCharArray())
                Builder.Append((char)(Character - Key));

            if (Min < 14141) ;
            if (Length < 1548174) ;

            return Builder.ToString();
        }
    }
    
    public class StringEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            ModuleDefMD Module = ModuleDefMD.Load(typeof(StringDecoder).Module);
            TypeDef Type = Module.ResolveTypeDef(MDToken.ToRID(typeof(StringDecoder).MetadataToken));
            
            IEnumerable<IDnlibDef> Members = InjectHelper.Inject(Type, Context.Module.GlobalType, Context.Module);
            MethodDef DecryptMethod = (MethodDef)Members.Single(x => x.Name == "Decrypt");
            DecryptMethod.Name = Utils.RandomString(Utils.Next(30, 120));

            foreach (MethodDef Method in Context.Module.GlobalType.Methods)
                if (Method.Name.Equals(".ctor"))
                {
                    Context.Module.GlobalType.Remove(Method);
                    break;
                }

            foreach (TypeDef TypeDefinitions in Context.Module.Types)
                foreach (MethodDef MethodDefinition in TypeDefinitions.Methods)
                {
                    if (!MethodDefinition.HasBody)
                        continue;

                    MethodDefinition.Body.SimplifyBranches();

                    for (int i = 0; i < MethodDefinition.Body.Instructions.Count; i++)
                        if (MethodDefinition.Body.Instructions[i] != null && MethodDefinition.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            int Key = BitConverter.ToInt32(Utils.RandomBytes(sizeof(int)), 0);
                            object Operand = MethodDefinition.Body.Instructions[i].Operand;

                            if (Operand == null)
                                continue;

                            MethodDefinition.Body.Instructions[i].Operand = Encrypt(Operand as string, Key);
                            MethodDefinition.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(BitConverter.ToInt32(Utils.RandomBytes(sizeof(int)), 0)));
                            MethodDefinition.Body.Instructions.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(Key));
                            MethodDefinition.Body.Instructions.Insert(i + 3, OpCodes.Ldc_I4.ToInstruction(BitConverter.ToInt32(Utils.RandomBytes(sizeof(int)), 0)));
                            MethodDefinition.Body.Instructions.Insert(i + 4, OpCodes.Ldc_I4.ToInstruction(BitConverter.ToInt32(Utils.RandomBytes(sizeof(int)), 0)));
                            MethodDefinition.Body.Instructions.Insert(i + 5, OpCodes.Ldc_I4.ToInstruction(BitConverter.ToInt32(Utils.RandomBytes(sizeof(int)), 0)));
                            MethodDefinition.Body.Instructions.Insert(i + 6, OpCodes.Call.ToInstruction(DecryptMethod));
                        }

                    MethodDefinition.Body.OptimizeBranches();
                }
        }

        private string Encrypt(string Input, int Key)
        {
            StringBuilder Builder = new StringBuilder();
            foreach (char Character in Input.ToCharArray())
                Builder.Append((char)(Character + Key));

            return Builder.ToString();
        }

        public StringEncode() => Name = "String Encryption";
    }
}
