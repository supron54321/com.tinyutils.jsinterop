using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class ParameterOperand : TypedOperand<ParameterDefinition>
    {
        public override ParameterDefinition Reference { get; }

        public override TypeReference Type => Reference.ParameterType;

        internal ParameterOperand(ParameterDefinition parameter) => Reference = parameter;
        public static implicit operator ParameterOperand(ParameterDefinition variable) => new ParameterOperand(variable);
        
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
                Instruction.Create(OpCodes.Ldarg, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Starg, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldarga, Reference)
            };
        }
    }
}