using System;
using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen
{
    public class TypeHelpersCollection
    {
        public readonly ModuleDefinition MainModule;

        public readonly MsgPackReaderHelper MsgPackReader;
        public readonly MsgPackWriterHelper MsgPackWriter;

        public readonly JsInteropUtilsHelper JsInteropUtils;
        public readonly JsBridgeHelper JsBridge;
        public readonly NativeByteArrayHelper NativeByteArray;
        public readonly MethodReference MonoPInvokeCallbackAttributeConstructor;
        public readonly MethodReference PreserveAttributeConstructor;

        public TypeHelpersCollection(ModuleDefinition mainModule)
        {
            MainModule = mainModule;
            
            MsgPackReader = new MsgPackReaderHelper(mainModule);
            MsgPackWriter = new MsgPackWriterHelper(mainModule);
            
            JsInteropUtils = new JsInteropUtilsHelper(mainModule);
            JsBridge = new JsBridgeHelper(mainModule);
            NativeByteArray = new NativeByteArrayHelper(mainModule);
            
            MonoPInvokeCallbackAttributeConstructor = mainModule.ImportReference(typeof(MonoPInvokeCallbackAttribute).GetConstructor(new Type[0]));
            PreserveAttributeConstructor = mainModule.ImportReference(typeof(PreserveAttribute).GetConstructor(new Type[0]));
        }
    }
}