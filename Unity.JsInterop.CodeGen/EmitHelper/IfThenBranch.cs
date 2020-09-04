using System;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class IfThenBranch{
        private ILProcessor _processor;
        private IfStatement _statement;
        internal IfThenBranch(IfStatement ifStatement, ILProcessor processor)
        {
            _statement = ifStatement;
            _processor = processor;
        }

        public void Else(Action<ILProcessor> elseAction)
        {
            _statement.ActivateElseBranch();
            elseAction?.Invoke(_processor);
            _processor.Append(_statement.EndInstruction);
        }
        public void Else(Action elseAction)
        {
            _statement.ActivateElseBranch();
            elseAction?.Invoke();
            _processor.Append(_statement.EndInstruction);
        }
    }
}