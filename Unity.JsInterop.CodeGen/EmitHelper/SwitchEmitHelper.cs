using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public static class SwitchEmitHelper
    {
        public static SwitchStatement Switch(this ILProcessor processor, OperandBase operand)
        {
            return new SwitchStatement(processor, operand);
        }
    }
}