using System;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class IfCondition
    {
        private ILProcessor _processor;
        private IfStatement _statement;
        internal IfCondition(IfStatement statement, ILProcessor processor)
        {
            _statement = statement;
            _processor = processor;
        }

        public IfThenBranch Then(Action<ILProcessor> thenAction)
        {
            thenAction?.Invoke(_processor);
            _processor.Append(_statement.ElseInstruction);
            return new IfThenBranch(_statement, _processor);
        }
        public IfThenBranch Then(Action thenAction)
        {
            thenAction?.Invoke();
            _processor.Append(_statement.ElseInstruction);
            return new IfThenBranch(_statement, _processor);
        }
    }
}