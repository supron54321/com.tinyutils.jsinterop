using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class SwitchStatement{
        ILProcessor _processor;
        OperandBase _valueCopy;

        Instruction _switchEnd;
        Instruction _currentCondition;
        Instruction _currentImplementation;

        MethodReference _equalityMethod;
        internal SwitchStatement(ILProcessor processor, OperandBase operand)
        {
            if (processor.Body.Method.Module == null)
                throw new MethodNotRegisteredException(
                    $"Method {processor.Body.Method} must be added to class and registered in module");
            _processor = processor;
            CopyValue(operand);

            if(!operand.Type.IsPrimitive)    // TODO: better type checking
                _equalityMethod = processor.Body.Method.Module.ImportReference(operand.Type.Resolve().Methods.First(method => method.Name == "op_Equality"));

            _currentCondition = Instruction.Create(OpCodes.Nop);
            _currentImplementation = Instruction.Create(OpCodes.Nop);
            _switchEnd = Instruction.Create(OpCodes.Nop);

            _valueCopy.EmitLoad(_processor);
            _processor.Emit(OpCodes.Brfalse, _switchEnd);

            _processor.Append(_currentCondition);
            _processor.Emit(OpCodes.Br, _switchEnd);
            _processor.Append(_currentImplementation);
            _processor.Append(_switchEnd);
        }

        private void CopyValue(OperandBase operand)
        {
            var copy = new VariableDefinition(operand.Type);
            _processor.Body.Variables.Add(copy);
            _valueCopy = copy;

            operand.EmitLoad(_processor);
            _valueCopy.EmitStore(_processor);
        }

        public SwitchStatement Case(ValueOperand operand, Action then)
        {
            return Case<int>(operand, then);
        }
        public SwitchStatement Case(ValueStringOperand operand, Action then)
        {
            return Case<string>(operand, then);
        }
        public SwitchStatement Case<T>(TypedOperand<T> operand, Action then)
        {
            then?.Invoke();
            _processor.Emit(OpCodes.Br, _switchEnd);
            var firstInstr = _switchEnd.Next;
            var condition = EmitCondition(operand, firstInstr);
            _processor.Remove(_switchEnd);
            _processor.Append(_switchEnd);
            foreach(var instr in condition)
            {
                _processor.InsertAfter(_currentCondition, instr);
                _currentCondition = instr;
            }
            return this;
        }

        private IEnumerable<Instruction> EmitCondition(OperandBase operand, Instruction implStart)
        {
            List<Instruction> instr = new List<Instruction>();
            instr.AddRange(_valueCopy.CreateLoad());
            instr.AddRange(operand.CreateLoad());
            instr.Add(CreateEqualityOperator(operand));
            instr.Add(Instruction.Create(OpCodes.Brtrue, implStart));
            return instr;
        }

        private Instruction CreateEqualityOperator(OperandBase operand){
            if(_equalityMethod != null)
                return Instruction.Create(OpCodes.Call, _equalityMethod);
            return Instruction.Create(OpCodes.Ceq);
        }
    }
}