using System.Reflection;
using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen
{
    public class JsBridgeHelper
    {
        private ModuleDefinition _mainModule;

        public TypeReference TypeRef { get; set; }
        public MethodReference RegisterJsCallback { get; set; }
        public MethodReference RegisterJsFunction { get; set; }
        public MethodReference CallJsFunction { get; set; }

        public JsBridgeHelper(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;

            var type = typeof(JsBridge);
            
            TypeRef = _mainModule.ImportReference(typeof(JsBridge));

            RegisterJsCallback =
                _mainModule.ImportReference(type.GetMethod(nameof(JsBridge.RegisterJsCallback), BindingFlags.Static | BindingFlags.Public));
            RegisterJsFunction =
                _mainModule.ImportReference(type.GetMethod(nameof(JsBridge.RegisterJsFunction), BindingFlags.Static | BindingFlags.Public));
            CallJsFunction =
                _mainModule.ImportReference(type.GetMethod(nameof(JsBridge.CallJsFunction), BindingFlags.Static | BindingFlags.Public));
        }
    }
}