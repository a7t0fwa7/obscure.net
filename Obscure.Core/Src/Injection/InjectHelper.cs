using System.Collections.Generic;
using System.Linq;

using dnlib.DotNet.Emit;
using dnlib.DotNet;

namespace Obscure.Core.Injection
{
    public static class InjectHelper
    {
        public static TypeDefUser Clone(TypeDef Origin)
        {
            TypeDefUser Type = new TypeDefUser(Origin.Namespace, Origin.Name)
            {
                Attributes = Origin.Attributes,
            };

            if (Origin.ClassLayout != null)
                Type.ClassLayout = new ClassLayoutUser(Origin.ClassLayout.PackingSize, Origin.ClassSize);

            foreach (GenericParam Parameter in Origin.GenericParameters)
                Type.GenericParameters.Add(new GenericParamUser(Parameter.Number, Parameter.Flags,
                    "-"));

            return Type;
        }

        public static MethodDefUser Clone(MethodDef Origin)
        {
            MethodDefUser Method = new MethodDefUser(Origin.Name, null, Origin.ImplAttributes,
                Origin.Attributes);

            foreach (GenericParam Parameter in Origin.GenericParameters)
                Method.GenericParameters.Add(new GenericParamUser(Parameter.Number, Parameter.Flags,
                    "-"));

            return Method;
        }

        public static FieldDefUser Clone(FieldDef Origin) =>
            new FieldDefUser(Origin.Name, null, Origin.Attributes);

        public static TypeDef PopulateContext(TypeDef TypeDefiniton, InjectContext Context)
        {
            TypeDef Type;
            if (!Context.Map.TryGetValue(TypeDefiniton, out IDnlibDef Existing))
            {
                Type = Clone(TypeDefiniton);
                Context.Map[TypeDefiniton] = Type;
            }
            else
                Type = (TypeDef)Existing;

            foreach (TypeDef NestedType in TypeDefiniton.NestedTypes)
                Type.NestedTypes.Add(PopulateContext(NestedType, Context));

            foreach (MethodDef Method in TypeDefiniton.Methods)
                Type.Methods.Add((MethodDef)(Context.Map[Method] = Clone(Method)));

            foreach (FieldDef Field in TypeDefiniton.Fields)
                Type.Fields.Add((FieldDef)(Context.Map[Field] = Clone(Field)));

            return Type;
        }

        public static void CopyTypeDef(TypeDef TypeDefiniton, InjectContext Context)
        {
            TypeDef Type = (TypeDef)Context.Map[TypeDefiniton];
            Type.BaseType = Context.Importer.Import(TypeDefiniton.BaseType);

            foreach (InterfaceImpl IFace in TypeDefiniton.Interfaces)
                Type.Interfaces.Add(new InterfaceImplUser(Context.Importer.Import(IFace.Interface)));
        }

        public static void CopyMethodDef(MethodDef _MethodDef, InjectContext Context)
        {
            MethodDef NewMethodDef = (MethodDef)Context.Map[_MethodDef];

            NewMethodDef.Signature = Context.Importer.Import(_MethodDef.Signature);
            NewMethodDef.Parameters.UpdateParameterTypes();

            if (_MethodDef.ImplMap != null)
                NewMethodDef.ImplMap = new ImplMapUser(new ModuleRefUser(Context.TargetModule, _MethodDef.ImplMap.Module.Name),
                    _MethodDef.ImplMap.Name, _MethodDef.ImplMap.Attributes);

            foreach (CustomAttribute Attribute in _MethodDef.CustomAttributes)
                NewMethodDef.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)Context.Importer.Import(Attribute.Constructor)));

            if (_MethodDef.HasBody)
            {
                NewMethodDef.Body = new CilBody(_MethodDef.Body.InitLocals, new List<Instruction>(), new List<ExceptionHandler>(), new List<Local>());
                NewMethodDef.Body.MaxStack = _MethodDef.Body.MaxStack;

                Dictionary<object, object> BodyMap = new Dictionary<object, object>();

                foreach (Local _Local in _MethodDef.Body.Variables)
                {
                    Local NewLocal = new Local(Context.Importer.Import(_Local.Type));
                    NewMethodDef.Body.Variables.Add(NewLocal);
                    NewLocal.Name = _Local.Name;
                    NewLocal.Attributes = _Local.Attributes;

                    BodyMap[_Local] = NewLocal;
                }

                foreach (Instruction _Instruction in _MethodDef.Body.Instructions)
                {
                    Instruction NewInstruction = new Instruction(_Instruction.OpCode, _Instruction.Operand)
                    {
                        SequencePoint = _Instruction.SequencePoint,
                    };

                    switch (NewInstruction.Operand)
                    {
                        case IType:
                            NewInstruction.Operand = Context.Importer.Import((IType)NewInstruction.Operand);
                            break;
                        case IMethod:
                            NewInstruction.Operand = Context.Importer.Import((IMethod)NewInstruction.Operand);
                            break;
                        case IField:
                            NewInstruction.Operand = Context.Importer.Import((IField)NewInstruction.Operand);
                            break;
                    }

                    NewMethodDef.Body.Instructions.Add(NewInstruction);
                    BodyMap[_Instruction] = NewInstruction;
                }

                foreach (Instruction _Instruction in NewMethodDef.Body.Instructions)
                {
                    if (_Instruction.Operand != null && BodyMap.ContainsKey(_Instruction.Operand))
                        _Instruction.Operand = BodyMap[_Instruction.Operand];

                    else if (_Instruction.Operand is Instruction[])
                        _Instruction.Operand = ((Instruction[])_Instruction.Operand).Select(Target => (Instruction)BodyMap[Target]).ToArray();
                }

                foreach (ExceptionHandler Handler in _MethodDef.Body.ExceptionHandlers)
                    NewMethodDef.Body.ExceptionHandlers.Add(new ExceptionHandler(Handler.HandlerType)
                    {
                        CatchType = Handler.CatchType == null ? null : Context.Importer.Import(Handler.CatchType),
                        TryStart = (Instruction)BodyMap[Handler.TryStart],
                        TryEnd = (Instruction)BodyMap[Handler.TryEnd],
                        HandlerStart = (Instruction)BodyMap[Handler.HandlerStart],
                        HandlerEnd = (Instruction)BodyMap[Handler.HandlerEnd],
                        FilterStart = Handler.FilterStart == null ? null : (Instruction)BodyMap[Handler.FilterStart]
                    });

                NewMethodDef.Body.SimplifyMacros(NewMethodDef.Parameters);
            }
        }

        public static void CopyFieldDef(FieldDef FieldDefinition, InjectContext Context)
        {
            FieldDef Field = (FieldDef)Context.Map[FieldDefinition];
            Field.Signature = Context.Importer.Import(FieldDefinition.Signature);
        }

        public static void Copy(TypeDef TypeDefiniton, InjectContext Context, bool CopySelf)
        {
            if (CopySelf)
                CopyTypeDef(TypeDefiniton, Context);

            foreach (TypeDef NestedType in TypeDefiniton.NestedTypes)
                Copy(NestedType, Context, true);

            foreach (MethodDef Method in TypeDefiniton.Methods)
                CopyMethodDef(Method, Context);

            foreach (FieldDef Field in TypeDefiniton.Fields)
                CopyFieldDef(Field, Context);
        }

        public static IEnumerable<IDnlibDef> Inject(TypeDef TypeDefiniton, TypeDef Type, ModuleDef Target)
        {
            InjectContext Context = new InjectContext(TypeDefiniton.Module, Target);
            Context.Map[TypeDefiniton] = Type;
            PopulateContext(TypeDefiniton, Context);
            Copy(TypeDefiniton, Context, false);

            return Context.Map.Values.Except(new[] { Type });
        }
    }
}