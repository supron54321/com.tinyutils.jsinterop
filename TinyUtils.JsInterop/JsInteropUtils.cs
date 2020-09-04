using System;
using TinyMsgPack;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace TinyUtils.JsInterop
{
    public struct JsError
    {
        public string Name;
        public string Message;
        public string Stack;
    }

    public static unsafe class JsInteropUtils
    {
        public static NativeArray<byte> JsDataPointerToNativeArray(IntPtr pointer)
        {
            int length = *(int*) pointer;
            NativeArray<byte> ret =
                NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>((void*) (pointer + 4), length,
                    Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref ret, AtomicSafetyHandle.Create());
#endif
            return ret;
        }

        public static void FreeJsDataAndArray(NativeArray<byte> array, IntPtr dataPointer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array));
#endif
            UnsafeUtility.Free((void*) dataPointer, Allocator.Invalid);
        }

        public static void DecodeErrorAndThrow(MsgPackReader reader, NativeArray<byte> asArray, IntPtr argsPtr)
        {
            if (!reader.ReadArrayHeader(out var returnArrayLen) || returnArrayLen != 2)
            {
                FreeJsDataAndArray(asArray, argsPtr);
                throw new JsInteropException($"Data returned from JS has invalid format");
            }

            if (DecodeError(reader, out JsError error))
            {
                FreeJsDataAndArray(asArray, argsPtr);
                throw new JavaScriptException(error);
            }
        }

        private static bool DecodeError(MsgPackReader reader, out JsError error)
        {
            error = default;
            if (!reader.ReadArrayHeader(out var errorLen) || errorLen != 3)
                return false;
            error.Name = reader.ReadString();
            error.Message = reader.ReadString();
            error.Stack = reader.ReadString();
            return true;
        }

        public static void AssertArgumentsCount(MsgPackReader reader, int count)
        {
            if (!reader.ReadArrayHeader(out var arrayLength) || arrayLength != count)
                throw new JsInteropException($"Invalid arguments count");
        }

        public static void WriteException(MsgPackWriter writer, Exception error)
        {
            if (error != null)
            {
                writer.WriteArrayHeader(3);
                writer.WriteString("DotnetException");
                writer.WriteString(error.Message);
                writer.WriteString(error.StackTrace);
            }
            else
            {
                writer.WriteNil();
            }
        }

        public static IntPtr PackSerializedData(MsgPackWriter writer)
        {
            IntPtr dataPtr = writer.GetUnsafeBufferPtr();
            *(int*) dataPtr = writer.GetBufferLength();
            return dataPtr;
        }
    }
}