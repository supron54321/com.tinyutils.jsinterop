using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace TinyUtils.JsInterop
{
    public static unsafe class JsBridge
    {
        private static bool _initialized = false;
        
        delegate IntPtr MallocWrapper(int size);
        
        [DllImport("__Internal")]
        static extern void _InitializeInterop(MallocWrapper allocFunctionPointer, IntPtr freeFunctionPointer);

        [DllImport("__Internal")]
        private static extern IntPtr _RegisterJsFunction(string path);

        [DllImport("__Internal")]
        private static extern IntPtr _CallJsFunction(IntPtr functionPointer, IntPtr arguments, int argumentsLength);

        [DllImport("__Internal")]
        private static extern void _RegisterJsCallback(string path, IntPtr functionPointer);

        
        public static IntPtr RegisterJsFunction(string path) 
        {
            if(!_initialized)
                Initialize();
            return _RegisterJsFunction(path);
        }

        public static IntPtr CallJsFunction(IntPtr functionPointer, IntPtr arguments, int argumentsLength)
            => _CallJsFunction(functionPointer, arguments, argumentsLength);
 
        public static void RegisterJsCallback(string path, IntPtr functionPointer)
        {
            if(!_initialized)
                Initialize();
            _RegisterJsCallback(path, functionPointer);
        }
        
        
        static void Initialize()
        {
            _initialized = true;
            Action<IntPtr> freeWrapper = Free;
            _InitializeInterop(MemAlloc, Marshal.GetFunctionPointerForDelegate(freeWrapper));
        }
        

        [MonoPInvokeCallback()]
        static IntPtr MemAlloc(int size)
        {
            return (IntPtr)UnsafeUtility.Malloc(size, 0, Allocator.Persistent);
        }
        [MonoPInvokeCallback()]
        static void Free(IntPtr address)
        {
            UnsafeUtility.Free((void*)address, Allocator.Persistent);
        }
    }
}