using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class VariableOperand : TypedOperand<VariableDefinition>
    {
        public override VariableDefinition Reference { get; }
        public override TypeReference Type => Reference.VariableType;

        internal VariableOperand(VariableDefinition reference) => Reference = reference;
        public static implicit operator VariableOperand(VariableDefinition variable) => new VariableOperand(variable);
        
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
                Instruction.Create(OpCodes.Ldloc, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Stloc, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldloca, Reference)
            };
        }
    }
}