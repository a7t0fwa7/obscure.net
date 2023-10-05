using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;

using Obscure.Core.Utility;

namespace Obscure.Core.Engine.Protections
{
    public class NameEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            Context.Module.Name = Utils.RandomString(Utils.Next(50, 70));

            foreach (TypeDef Type in Context.Module.Types)
            {
                if (Type is { IsRuntimeSpecialName: false, IsGlobalModuleType: false })
                {
                    Type.Namespace = Utils.RandomString(Utils.Next(50, 70));
                    Type.Name = Utils.RandomString(Utils.Next(50, 70));
                }

                foreach (FieldDef Field in Type.Fields)
                {
                    if (Field is { IsRuntimeSpecialName: false, DeclaringType.IsEnum: false, IsLiteral: false })
                        Field.Name = Utils.RandomString(Utils.Next(50, 70));
                }

                foreach (PropertyDef Property in Type.Properties)
                {
                    if (Property is { IsRuntimeSpecialName: false })
                        Property.Name = Utils.RandomString(Utils.Next(50, 70));
                }

                foreach (EventDef Event in Type.Events)
                {
                    if (Event is { IsRuntimeSpecialName: false })
                        Event.Name = Utils.RandomString(Utils.Next(50, 70));
                }

                foreach (MethodDef Method in Type.Methods)
                {
                    if (Method is { IsRuntimeSpecialName: false, DeclaringType.IsForwarder: false })
                        Method.Name = Utils.RandomString(Utils.Next(50, 70));

                    foreach (Parameter Parameter in Method.Parameters)
                        if (!Parameter.IsHiddenThisParameter)
                            Parameter.Name = Utils.RandomString(Utils.Next(50, 70));
                }
            }
        }

        public NameEncode() => Name = "Renaming Encryption";
    }
}
