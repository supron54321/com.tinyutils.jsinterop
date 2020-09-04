using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public static class IfEmitHelper
    {
        public static IfStatement If(this ILProcessor processor, OperandBase op)
        {
            return new IfStatement(processor, op);
        }

        public static void Test()
        {
            MethodBody m = null;
            var il = m.GetILProcessor();
            var testVar = new VariableDefinition(null);
            var param1 = new ParameterDefinition(null);
            var field = new FieldDefinition(null, FieldAttributes.Public, testVar.VariableType);
            il.If((testVar, field)).Eq(param1).Then((processor) =>{
                
            }).Else((processor)=>{

            });
        }
    }
}