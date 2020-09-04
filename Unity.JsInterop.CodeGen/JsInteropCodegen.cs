using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace TinyUtils.JsInterop.CodeGen
{
    public class JsInteropCodegen
    {
        private ModuleDefinition _mainModule;

        private TypeHelpersCollection _typeHelper;
        
        public JsInteropCodegen(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;
            
            _typeHelper = new TypeHelpersCollection(mainModule);
        }


        public void ProcessAssembly()
        {
            var callbackMethods = _mainModule.GetTypes().SelectMany(item => item.Methods).Where(method =>
                method.HasCustomAttributes &&
                method.CustomAttributes.Any(attr => attr.AttributeType.Name == nameof(JsCallback)));
            var functionMethods = _mainModule.GetTypes().SelectMany(item => item.Methods).Where(method =>
                method.HasCustomAttributes &&
                method.CustomAttributes.Any(attr => attr.AttributeType.Name == nameof(JsFunction)));
            
            SerializationClassCodegen serialization = new SerializationClassCodegen(_mainModule, _typeHelper);
            
            CallbackCodegen callbackCodegen = new CallbackCodegen(_typeHelper, serialization);
            foreach (var callbackMethod in callbackMethods)
            {
                callbackCodegen.ProcessMethod(callbackMethod);
            }
            
            FunctionCodegen functionCodegen = new FunctionCodegen(_typeHelper, serialization);
            foreach (var function in functionMethods)
            {
                functionCodegen.ProcessMethod(function);
            }
        }
    }
}