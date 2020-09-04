using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TinyUtils.JsInterop.CodeGen.Helper;
using Unity.Collections;

namespace TinyUtils.JsInterop.CodeGen
{
    public class FunctionCodegen
    {
        private TypeHelpersCollection _typeHelper;
        private SerializationClassCodegen _serialization;

        public FunctionCodegen(TypeHelpersCollection typeHelper, SerializationClassCodegen serialization)
        {
            _typeHelper = typeHelper;
            _serialization = serialization;
        }
        
        public void ProcessMethod(MethodDefinition method)
        {
            var functionAttr =
                method.CustomAttributes.First(item => item.AttributeType.FullName == typeof(JsFunction).FullName);
            BuildMethodBody(method, functionAttr);
        }

        private void BuildMethodBody(MethodDefinition method, CustomAttribute functionAttr)
        {
            var ilProcessor = method.Body.GetILProcessor();

            var writerVar = ilProcessor.AddVariable(_typeHelper.MsgPackWriter.TypeRef);
            var functionPtr = BuildInitializer(method, functionAttr);
            
            
            ilProcessor.Emit(OpCodes.Ldloca_S, writerVar);
            ilProcessor.Emit(OpCodes.Ldc_I4, (int) Allocator.Temp);
            ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackWriter.Ctor);

            ilProcessor.CallMethod(writerVar, _typeHelper.MsgPackWriter.WriteArrayHeader, method.Parameters.Count);

            foreach (var param in method.Parameters)
            {
                var serializeMethod = _serialization.GetTypeSerializeMethod(param.ParameterType);
                ilProcessor.CallStatic(serializeMethod, param, writerVar);
            }

            var paramsPtr = ilProcessor.AddVariable(_typeHelper.MainModule.TypeSystem.IntPtr);
            var paramsLen = ilProcessor.AddVariable(_typeHelper.MainModule.TypeSystem.Int32);
            
            var returnPtr = ilProcessor.AddVariable(_typeHelper.MainModule.TypeSystem.IntPtr);
            
            ilProcessor.CallMethod(writerVar, _typeHelper.MsgPackWriter.GetUnsafeBufferPtr).Store(paramsPtr);
            ilProcessor.CallMethod(writerVar, _typeHelper.MsgPackWriter.GetBufferLength).Store(paramsLen);
            ilProcessor.CallStatic(_typeHelper.JsBridge.CallJsFunction, functionPtr, paramsPtr, paramsLen).Store(returnPtr);
            
            ilProcessor.Emit(OpCodes.Ret);
        }

        private FieldDefinition BuildInitializer(MethodDefinition method, CustomAttribute functionAttr)
        {
            var wrapperClass = new TypeDefinition(method.DeclaringType.Namespace, method.Name+"_FunctionInitializer_", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed, method.Module.TypeSystem.Object);
            method.Module.Types.Add(wrapperClass);
            var fcnPtrField = new FieldDefinition("FcnPtr", FieldAttributes.Static | FieldAttributes.Public,
                method.Module.TypeSystem.IntPtr);
            wrapperClass.Fields.Add(fcnPtrField);

            var constructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static, _typeHelper.MainModule.TypeSystem.Void);
            wrapperClass.Methods.Add(constructor);

            var ilProcessor = constructor.Body.GetILProcessor();

            ilProcessor.CallStatic(_typeHelper.JsBridge.RegisterJsFunction,
                functionAttr.ConstructorArguments[0].Value as string).Store(fcnPtrField);
            //ilProcessor.Emit(OpCodes.Stsfld, fcnPtrField);
            ilProcessor.Emit(OpCodes.Ret);
            
            return fcnPtrField;
        }
    }
}