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
    public class IntEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            IMethod AbsMethod = Context.Module.Import(typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }));
            IMethod MinMethod = Context.Module.Import(typeof(Math).GetMethod("Min", new Type[] { typeof(int), typeof(int) }));

            foreach (TypeDef Type in Context.Module.Types)
                foreach (MethodDef Method in Type.Methods)
                {
                    if (!Method.HasBody)
                        continue;

                    for (int i = 0; i < Method.Body.Instructions.Count; i++)
                        if (Method.Body.Instructions[i] != null && Method.Body.Instructions[i].IsLdcI4())
                        {
                            int Operand = Method.Body.Instructions[i].GetLdcI4Value();
                            if (Operand <= 0)
                                continue;

                            Method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(AbsMethod));

                            int Negative = Utils.Next(30, 120);
                            if (Negative % 2 != 0)
                                Negative += 1;

                            for (var j = 0; j < Negative; j++)
                                Method.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));

                            if (Operand < int.MaxValue)
                            {
                                Method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
                                Method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(MinMethod));
                            }
                        }
                }
        }

        public IntEncode() => Name = "Integer Encoding";
    }
}
