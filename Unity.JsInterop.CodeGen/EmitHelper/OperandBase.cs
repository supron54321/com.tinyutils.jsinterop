using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public abstract class OperandBase
    {
        public abstract Instruction Emit(ILProcessor processor, OpCode opCode);
        public abstract Instruction Create(OpCode opCode);

        public abstract TypeReference Type {get;}
        
        public abstract IEnumerable<Instruction> CreateLoad();
        public abstract IEnumerable<Instruction> CreateStore();

        public abstract IEnumerable<Instruction> CreateLoadAddr();
        
        public static implicit operator OperandBase(VariableDefinition variable) => new VariableOperand(variable);
        public static implicit operator OperandBase(ParameterDefinition variable) => new ParameterOperand(variable);
        public static implicit operator OperandBase((OperandBase, FieldDefinition) variable) => new FieldOperand(variable.Item1, variable.Item2);
        public static implicit operator OperandBase(FieldDefinition variable) => new StaticFieldOperand(variable);
        public static implicit operator OperandBase(int value) => new ValueOperand(value);
        public static implicit operator OperandBase(string value) => new ValueStringOperand(value);

        public IEnumerable<Instruction> EmitLoad(ILProcessor processor)
        {
            var instructions = CreateLoad();
            foreach(var instr in instructions) processor.Append(instr);
            return instructions;
        }
        public IEnumerable<Instruction> EmitStore(ILProcessor processor)
        {
            var instructions = CreateStore();
            foreach(var instr in instructions) processor.Append(instr);
            return instructions;
        }
        public IEnumerable<Instruction> EmitLoadAddr(ILProcessor processor)
        {
            var instructions = CreateLoadAddr();
            foreach(var instr in instructions) processor.Append(instr);
            return instructions;
        }
    }
}