using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class ForStatement
    {
        ILProcessor _processor;
        OperandBase Iterator { get; }
        IEnumerable<Instruction> ConditionCode { get; }
        Instruction _loopStart;
        Instruction _increaseInstr;
        Instruction _conditionStart;
        Instruction _loopEnd;

        internal ForStatement(ILProcessor processor, int from, OperandBase to, int inc)
        {
            if (processor.Body.Method.Module == null)
                throw new MethodNotRegisteredException(
                    $"Method {processor.Body.Method} must be added to class and registered in module");
            _processor = processor;
            var iterator = processor.AddVariable(processor.Body.Method.Module.TypeSystem.Int32);
            Iterator = iterator;
            EmitIteratorInitialization(from);

            _loopStart = Instruction.Create(OpCodes.Nop);
            ConditionCode = CreateConditionCode(to, inc);
            _processor.Emit(OpCodes.Br, _conditionStart);
            _processor.Append(_loopStart);
            _loopEnd = Instruction.Create(OpCodes.Nop);
        }

        private IEnumerable<Instruction> CreateConditionCode(OperandBase to, int inc)
        {
            List<Instruction> code = new List<Instruction>();
            var iteratorLoad = Iterator.CreateLoad();
            _increaseInstr = iteratorLoad.First();
            code.AddRange(iteratorLoad);
            code.AddRange(new ValueOperand(inc).CreateLoad());
            code.Add(Instruction.Create(OpCodes.Add));
            code.AddRange(Iterator.CreateStore());

            iteratorLoad = Iterator.CreateLoad();
            _conditionStart = iteratorLoad.First();
            code.AddRange(iteratorLoad);
            code.AddRange(to.CreateLoad());
            code.Add(Instruction.Create(OpCodes.Clt));
            code.Add(Instruction.Create(OpCodes.Brtrue, _loopStart));
            return code;
        }

        private void EmitIteratorInitialization(int from)
        {
            new ValueOperand(from).EmitLoad(_processor);
            Iterator.EmitStore(_processor);
        }

        public void Then(Action<ILProcessor> then)
        {
            then?.Invoke(_processor);
            foreach (var instruction in ConditionCode)
                _processor.Append(instruction);
            _processor.Append(_loopEnd);
        }

        public void Then(Action then)
        {
            then?.Invoke();
            foreach (var instruction in ConditionCode)
                _processor.Append(instruction);
            _processor.Append(_loopEnd);
        }
    }
}