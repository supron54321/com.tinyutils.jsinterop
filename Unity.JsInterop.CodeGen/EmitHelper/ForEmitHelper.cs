using Mono.Cecil.Cil;
using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public static class ForEmitHelper
    {
        public static ForStatement For(this ILProcessor processor, int from, OperandBase to, int inc)
        {
            return new ForStatement(processor, from, to, inc);
        }
    }
}