using System.Linq;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class IfStatement
    {
        private ILProcessor _processor;
        internal Instruction StartInstruction {get;}
        internal Instruction ElseInstruction {get;}
        internal Instruction EndInstruction {get;}

        internal IfStatement(ILProcessor processor, OperandBase operand)
        {
            _processor = processor;
            StartInstruction = operand.EmitLoad(_processor).First();
            ElseInstruction = Instruction.Create(OpCodes.Nop);
            EndInstruction = Instruction.Create(OpCodes.Nop);
        }

        public IfCondition Null()
        {
            _processor.Emit(OpCodes.Ldnull);
            _processor.Emit(OpCodes.Ceq);
            _processor.Emit(OpCodes.Brfalse, ElseInstruction);
            return new IfCondition(this, _processor);
        }
        public IfCondition True()
        {
            _processor.Emit(OpCodes.Brfalse, ElseInstruction);
            return new IfCondition(this, _processor);
        }

        public IfCondition Eq(OperandBase operand) => EmitCondition(operand, OpCodes.Ceq);
        public IfCondition Gt(OperandBase operand) => EmitCondition(operand, OpCodes.Cgt);
        public IfCondition Lt(OperandBase operand) => EmitCondition(operand, OpCodes.Clt);

        private IfCondition EmitCondition(OperandBase operand, OpCode conditionCode)
        {
            operand.EmitLoad(_processor);
            _processor.Emit(conditionCode);
            _processor.Emit(OpCodes.Brfalse, ElseInstruction);
            return new IfCondition(this, _processor);
        }

        internal void ActivateElseBranch()
        {
            _processor.InsertBefore(ElseInstruction, Instruction.Create(OpCodes.Br, EndInstruction));
        }
    }
}