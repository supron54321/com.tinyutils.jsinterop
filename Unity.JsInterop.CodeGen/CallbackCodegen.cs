using System;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TinyUtils.JsInterop.CodeGen.Helper;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace TinyUtils.JsInterop.CodeGen
{
    public class CallbackCodegen
    {
        private TypeHelpersCollection _typeHelper;
        private SerializationClassCodegen _serialization;
        
        public CallbackCodegen(TypeHelpersCollection typeHelper, SerializationClassCodegen serialization)
        {
            _typeHelper = typeHelper;
            _serialization = serialization;
        }

        public void ProcessMethod(MethodDefinition method)
        {
            var wrapper = BuildWrapper(method);
            var wrapperMethod = CreateWrapperMethod(method, wrapper);
            var initializer = CreateInitializer(method, wrapperMethod);
        }

        private TypeDefinition CreateInitializer(MethodDefinition method, MethodDefinition wrapperMethod)
        {
            var initializerType = new TypeDefinition(method.DeclaringType.Namespace,
                $"{method.Name}_CallbackInitializer_",
                TypeAttributes.Abstract| TypeAttributes.Sealed |TypeAttributes.Class | TypeAttributes.Public,
                _typeHelper.MainModule.TypeSystem.Object);
            initializerType.CustomAttributes.Add(new CustomAttribute(_typeHelper.PreserveAttributeConstructor));
            
            _typeHelper.MainModule.Types.Add(initializerType);
            var fcnConstruct = _typeHelper.MainModule.ImportReference(typeof(Func<IntPtr, IntPtr>).GetConstructor(new []{typeof(object), typeof(IntPtr)}));
            var wrapper =
                _typeHelper.MainModule.ImportReference(
                    typeof(Marshal)).Resolve().Methods.First(m => m.Name == nameof(Marshal.GetFunctionPointerForDelegate));
            var wrapper2 = _typeHelper.MainModule.ImportReference(wrapper);
            
            var callbackAttr =
                method.CustomAttributes.First(item => item.AttributeType.FullName == typeof(JsCallback).FullName);
            var methodPath = callbackAttr.ConstructorArguments[0].Value as string;
            
            var constructor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static, _typeHelper.MainModule.TypeSystem.Void);
            initializerType.Methods.Add(constructor);
            
            
#if UNITY_EDITOR
            constructor.CustomAttributes.Add(new CustomAttribute(_typeHelper.MainModule.ImportReference(typeof(RuntimeInitializeOnLoadMethodAttribute).GetConstructors()[0])));
#endif
            
            var ilProcessor = constructor.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldstr, methodPath);
            ilProcessor.Emit(OpCodes.Ldnull);
            ilProcessor.Emit(OpCodes.Ldftn, wrapperMethod);
            ilProcessor.Emit(OpCodes.Newobj, fcnConstruct);
            ilProcessor.Emit(OpCodes.Call, wrapper2);
            ilProcessor.Emit(OpCodes.Call, _typeHelper.JsBridge.RegisterJsCallback);
            ilProcessor.Emit(OpCodes.Ret);
            return initializerType;
        }

        private MethodDefinition CreateWrapperMethod(MethodDefinition method, TypeDefinition wrapper)
        {
            var wrapperMethod = new MethodDefinition("CallbackWrapper", MethodAttributes.Public | MethodAttributes.Static,
                _typeHelper.MainModule.TypeSystem.IntPtr);
            wrapper.Methods.Add(wrapperMethod);
            wrapperMethod.CustomAttributes.Add(new CustomAttribute(_typeHelper.MonoPInvokeCallbackAttributeConstructor));

            var argsPtrParam =
                new ParameterDefinition("argsPtr", ParameterAttributes.None, _typeHelper.MainModule.TypeSystem.IntPtr);
            wrapperMethod.Parameters.Add(argsPtrParam);

            var ilProcessor = wrapperMethod.Body.GetILProcessor();
            if (method.ReturnType.FullName != "System.Void")
            {
                var resultVar = ilProcessor.AddVariable(method.ReturnType);
                var exceptionCatchVar = ilProcessor.AddVariable(_typeHelper.MainModule.ImportReference(typeof(Exception)));
                var errorVar = ilProcessor.AddVariable(_typeHelper.MainModule.ImportReference(typeof(Exception)));
                var arrayVar = ilProcessor.AddVariable(_typeHelper.NativeByteArray.TypeRef);
                var readerVar = ilProcessor.AddVariable(_typeHelper.MsgPackReader.TypeRef);
                var writerVar = ilProcessor.AddVariable(_typeHelper.MsgPackWriter.TypeRef);

                OperandBase[] argsVars = new OperandBase[method.Parameters.Count];
                for (int i = 0; i < argsVars.Length; i++)
                {
                    argsVars[i] = ilProcessor.AddVariable(method.Parameters[i].ParameterType);
                }

                if (method.ReturnType.IsValueType)
                {
                    ilProcessor.Emit(OpCodes.Ldloca, resultVar);
                    ilProcessor.Emit(OpCodes.Initobj, method.ReturnType);
                }
                else
                {
                    ilProcessor.Emit(OpCodes.Ldnull);
                    ilProcessor.Emit(OpCodes.Stloc, resultVar);
                }


                ilProcessor.Emit(OpCodes.Ldnull);
                ilProcessor.Emit(OpCodes.Stloc, errorVar);

                // Try block start
                ilProcessor.Emit(OpCodes.Ldarg, argsPtrParam);
                var tryBlockStart = ilProcessor.Body.Instructions.Last();
                ilProcessor.Emit(OpCodes.Call, _typeHelper.JsInteropUtils.JsDataPointerToNativeArray);
                ilProcessor.Emit(OpCodes.Stloc, arrayVar);

                ilProcessor.Emit(OpCodes.Ldloc, arrayVar);
                ilProcessor.Emit(OpCodes.Newobj, _typeHelper.MsgPackReader.Ctor);
                ilProcessor.Emit(OpCodes.Stloc, readerVar);

                ilProcessor.CallStatic(_typeHelper.JsInteropUtils.AssertArgumentsCount, readerVar, method.Parameters.Count);

                for (int i = 0; i < argsVars.Length; i++)
                {
                    var deserializeMethod = _serialization.GetTypeDeserializationMethod(argsVars[i].Type);
                    ilProcessor.CallStatic(deserializeMethod, readerVar).Store(argsVars[i]);
                }

                ilProcessor.CallStatic(_typeHelper.JsInteropUtils.FreeJsDataAndArray, arrayVar, argsPtrParam);
                ilProcessor.CallStatic(method, argsVars).Store(resultVar);;

                var tryBlockEnd = ilProcessor.Body.Instructions.Last();
                // catch block
                ilProcessor.Emit(OpCodes.Stloc, exceptionCatchVar);
                var catchBlockStart = ilProcessor.Body.Instructions.Last();
                ilProcessor.Emit(OpCodes.Ldloc, exceptionCatchVar);
                ilProcessor.Emit(OpCodes.Stloc, errorVar);
                var catchBlockEnd = ilProcessor.Body.Instructions.Last();
                // exception end
                var exceptionBlockEnd = Instruction.Create(OpCodes.Nop);
                ilProcessor.Append(exceptionBlockEnd);
                ilProcessor.InsertAfter(tryBlockEnd, Instruction.Create(OpCodes.Leave_S, exceptionBlockEnd));
                ilProcessor.InsertAfter(catchBlockEnd, Instruction.Create(OpCodes.Leave_S, exceptionBlockEnd));

                // Initialize result writer

                ilProcessor.Emit(OpCodes.Ldloca_S, writerVar);
                ilProcessor.Emit(OpCodes.Ldc_I4, (int) Allocator.Temp);
                ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackWriter.Ctor);

                ilProcessor.Emit(OpCodes.Ldloca_S, writerVar);
                ilProcessor.Emit(OpCodes.Ldc_I4, int.MaxValue);
                ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackWriter.WriteRawBigEndian32);

                ilProcessor.Emit(OpCodes.Ldloca_S, writerVar);
                ilProcessor.Emit(OpCodes.Ldc_I4_2);
                ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackWriter.WriteArrayHeader);
                
                ilProcessor.CallStatic(_typeHelper.JsInteropUtils.WriteException, writerVar, errorVar);

                ilProcessor.If(errorVar).Null().Then(()=>{
                    var serializationMethod = _serialization.GetTypeSerializeMethod(resultVar.VariableType);
                    ilProcessor.CallStatic(serializationMethod, resultVar, writerVar);
                }).Else(() =>{
                    ilProcessor.Emit(OpCodes.Ldloca_S, writerVar);
                    ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackWriter.WriteNil);
                });
                // Write output data to heap
                ilProcessor.CallStatic(_typeHelper.JsInteropUtils.PackSerializedData, writerVar);

                ilProcessor.Emit(OpCodes.Ret);


                wrapperMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
                {
                    TryStart = tryBlockStart,
                    TryEnd = tryBlockEnd.Next.Next,
                    HandlerStart = catchBlockStart,
                    HandlerEnd = catchBlockEnd.Next.Next,
                    CatchType = _typeHelper.MainModule.ImportReference(typeof(Exception))
                });
            }

            return wrapperMethod;
        }

        private TypeDefinition BuildWrapper(MethodDefinition method)
        {
            var wrapper = CreateWrapperClass(method);
            return wrapper;
        }

        private TypeDefinition CreateWrapperClass(MethodDefinition method)
        {
            var type = new TypeDefinition(method.DeclaringType.Namespace,
                $"{method.Name}_Callback_Wrapper",
                TypeAttributes.NestedPublic | TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed,
                method.Module.TypeSystem.Object);
            method.DeclaringType.NestedTypes.Add(type);
            return type;
        }
    }
}