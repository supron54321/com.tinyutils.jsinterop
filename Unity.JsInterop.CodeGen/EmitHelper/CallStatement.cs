using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class CallStatement
    {
        ILProcessor _processor;
        internal CallStatement(ILProcessor processor, MethodReference method, params OperandBase[] parameters)
        {
            if (method.Resolve()?.IsStatic == false)
                throw new ArgumentException($"Method is not static", nameof(method));
            _processor = processor;
            foreach (var p in parameters)
                p.EmitLoad(processor);
            processor.Emit(OpCodes.Call, method);
        }
        internal CallStatement(ILProcessor processor, OperandBase obj, MethodReference method, params OperandBase[] parameters)
        {
            var resolved = method.Resolve();
            if (resolved?.IsStatic == true)
                throw new ArgumentException($"Method is static", nameof(method));
            _processor = processor;
            if (obj.Type.IsValueType)
                obj.EmitLoadAddr(processor);
            else
                obj.EmitLoad(processor);

            foreach (var p in parameters)
                p.EmitLoad(processor);
            if (obj.Type.IsValueType)
                processor.Emit(OpCodes.Call, method);
            else
                processor.Emit(OpCodes.Callvirt, method);
        }

        public void Store(OperandBase operand)
        {
            operand.EmitStore(_processor);
        }
    }
}