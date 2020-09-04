using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class FieldOperand : TypedOperand<FieldDefinition>
    {
        public OperandBase Container { get; }
        public override FieldDefinition Reference { get; }

        public override TypeReference Type => Reference.FieldType;

        internal FieldOperand(OperandBase container, FieldDefinition field)
        {
            if(field.IsStatic)
                throw new ArgumentException($"Field {field} is static", nameof(field));
            if(container.Type != field.DeclaringType)
                throw new ArgumentException($"Field {field} is not declared in {container.Type}", nameof(field));
            Container = container;
            Reference = field;
        }
        public static implicit operator FieldOperand((OperandBase, FieldDefinition) variable) => new FieldOperand(variable.Item1, variable.Item2);

        public override Instruction Emit(ILProcessor processor, OpCode opCode)
        {
            var instruction = Instruction.Create(opCode, Reference);
            processor.Append(instruction);
            return instruction;
        }
        public override Instruction Create(OpCode opCode) => Instruction.Create(opCode, Reference);

        public override IEnumerable<Instruction> CreateLoad()
        {
            return new[]
            {
                Container.CreateLoad().First(),
                Instruction.Create(OpCodes.Ldfld, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            return new[]
            {
                Container.CreateLoad().First(),
                Instruction.Create(OpCodes.Stfld, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            return new[]
            {
                Container.CreateLoad().First(),
                Instruction.Create(OpCodes.Ldflda, Reference)
            };
        }
    }
}