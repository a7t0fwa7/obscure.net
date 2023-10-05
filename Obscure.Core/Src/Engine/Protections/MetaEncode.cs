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
    public class MetaEncode : Protection
    {
        public override void Run(ObfuscationContext Context)
        {
            foreach (CustomAttribute Attributes in Context.Module.Assembly.CustomAttributes)
                Context.Module.Assembly.CustomAttributes.Remove(Attributes);

            Context.Module.Mvid = null;
            Context.Module.Name = null;

            foreach (TypeDef Type in Context.Module.Types)
            {
                foreach (CustomAttribute Attributes in Type.CustomAttributes)
                    if (Type is { IsRuntimeSpecialName: false, IsGlobalModuleType: false })
                        Type.CustomAttributes.Remove(Attributes);

                foreach (MethodDef Method in Type.Methods)
                {
                    foreach (CustomAttribute Attributes in Method.CustomAttributes)
                        if (Method is { IsRuntimeSpecialName: false, DeclaringType.IsForwarder: false })
                            Method.CustomAttributes.Remove(Attributes);
                }
                
                foreach (PropertyDef Properties in Type.Properties)
                {
                    foreach (CustomAttribute Attributes in Properties.CustomAttributes)
                        if (Properties is { IsRuntimeSpecialName: false })
                            Properties.CustomAttributes.Remove(Attributes);
                }

                foreach (FieldDef Field in Type.Fields)
                {
                    foreach (CustomAttribute Attributes in Field.CustomAttributes)
                        if (Field is { IsRuntimeSpecialName: false, DeclaringType.IsEnum: false, IsLiteral: false })
                            Field.CustomAttributes.Remove(Attributes);
                }

                foreach (EventDef Events in Type.Events)
                {
                    foreach (CustomAttribute Attributes in Events.CustomAttributes)
                        if (Events is { IsRuntimeSpecialName: false })
                            Events.CustomAttributes.Remove(Attributes);
                }
            }
        }
        
        public MetaEncode() => Name = "Invalid Metadata";
    }
}
