using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class StaticFieldOperand : TypedOperand<FieldDefinition>
    {
        public override FieldDefinition Reference { get; }

        public override TypeReference Type => Reference.FieldType;

        internal StaticFieldOperand(FieldDefinition field)
        {
            if(!field.IsStatic)
                throw new ArgumentException($"Field {field} is not static", nameof(field));
            Reference = field;
        }
        public static implicit operator StaticFieldOperand(FieldDefinition variable) => new StaticFieldOperand(variable);

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
                Instruction.Create(OpCodes.Ldsfld, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Stsfld, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldsflda, Reference)
            };
        }
    }
}