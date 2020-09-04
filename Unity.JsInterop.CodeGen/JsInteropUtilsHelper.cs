using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen
{
    public class JsInteropUtilsHelper
    {
        private ModuleDefinition _mainModule;

        public TypeReference TypeRef { get; }

        public MethodReference JsDataPointerToNativeArray { get; }
        public MethodReference FreeJsDataAndArray { get; }
        public MethodReference DecodeErrorAndThrow { get; }
        public MethodReference AssertArgumentsCount { get; }
        public MethodReference WriteException { get; }
        public MethodReference PackSerializedData { get; }

        public JsInteropUtilsHelper(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;

            var type = typeof(JsInteropUtils);
            
            TypeRef = _mainModule.ImportReference(typeof(JsInteropUtils));

            JsDataPointerToNativeArray =
                _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.JsDataPointerToNativeArray)));
            FreeJsDataAndArray = _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.FreeJsDataAndArray)));
            DecodeErrorAndThrow =
                _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.DecodeErrorAndThrow)));
            AssertArgumentsCount =
                _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.AssertArgumentsCount)));
            WriteException = _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.WriteException)));
            PackSerializedData = _mainModule.ImportReference(type.GetMethod(nameof(JsInteropUtils.PackSerializedData)));
        }
    }
}