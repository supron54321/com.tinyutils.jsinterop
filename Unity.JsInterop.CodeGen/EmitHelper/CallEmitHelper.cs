using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public static class CallEmitHelper
    {
        public static CallStatement CallStatic(this ILProcessor processor, MethodReference method, params OperandBase[] parameters)
        {
            return new CallStatement(processor, method, parameters);
        }
        public static CallStatement CallMethod(this ILProcessor processor, OperandBase obj, MethodReference method, params OperandBase[] parameters)
        {
            return new CallStatement(processor, obj, method, parameters);
        }
    }
}