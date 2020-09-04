using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class ValueOperand : TypedOperand<int>
    {
        public override int Reference { get; }
        public override TypeReference Type => throw new NotImplementedException();

        internal ValueOperand(int reference) => Reference = reference;
        public static implicit operator ValueOperand(int variable) => new ValueOperand(variable);
        
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
                Instruction.Create(OpCodes.Ldc_I4, Reference)
            };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            throw new NotSupportedException($"Value operand does not support store operation");
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            throw new NotSupportedException($"Value operand does not support load address operation");
        }
    }
    public class ValueStringOperand : TypedOperand<string>
    {
        public override string Reference { get; }
        public override TypeReference Type => throw new NotImplementedException();

        internal ValueStringOperand(string reference) => Reference = reference;
        public static implicit operator ValueStringOperand(string variable) => new ValueStringOperand(variable);
        
        public override Instruction Emit(ILProcessor processor, OpCode opCode)
        {
            var instruction = Instruction.Create(opCode, Reference);
            processor.Append(instruction);
            return instruction;
        }

        public override Instruction Create(OpCode opCode) => Instruction.Create(opCode, Reference);

        public override IEnumerable<Instruction> CreateLoad()
        {
            if(Reference != null)
                return new[] { Instruction.Create(OpCodes.Ldstr, Reference) };
            return new[] { Instruction.Create(OpCodes.Ldnull) };
        }

        public override IEnumerable<Instruction> CreateStore()
        {
            throw new NotSupportedException($"Value operand does not support store operation");
        }

        public override IEnumerable<Instruction> CreateLoadAddr()
        {
            throw new NotSupportedException($"Value operand does not support load address operation");
        }
    }
}