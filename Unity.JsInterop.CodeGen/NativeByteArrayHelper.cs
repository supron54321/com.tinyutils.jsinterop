using Mono.Cecil;
using Unity.Collections;

namespace TinyUtils.JsInterop.CodeGen
{
    public class NativeByteArrayHelper
    {
        private ModuleDefinition _mainModule;
        
        public TypeReference TypeRef { get; }
        
        public NativeByteArrayHelper(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;

            TypeRef = _mainModule.ImportReference(typeof(NativeArray<byte>));
        }
    }
}