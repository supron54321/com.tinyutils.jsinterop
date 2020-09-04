using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;
using TinyMsgPack;
using Unity.Collections;
using UnityEngine;

namespace TinyUtils.JsInterop.CodeGen
{
    public class MsgPackWriterHelper
    {
        private ModuleDefinition _mainModule;
        public TypeReference TypeRef { get; }
        public MethodReference Ctor { get; }
        
        // Getters
        public MethodReference GetBufferLength { get; }
        public MethodReference GetUnsafeBufferPtr { get; }
        
        // Writers
        public MethodReference WriteArrayHeader { get; }
        public MethodReference WriteMapHeader { get; }
        public MethodReference WriteInteger8 { get; }
        public MethodReference WriteIntegerU8 { get; }
        public MethodReference WriteInteger16 { get; }
        public MethodReference WriteIntegerU16 { get; }
        public MethodReference WriteInteger32 { get; }
        public MethodReference WriteIntegerU32 { get; }
        public MethodReference WriteInteger64 { get; }
        public MethodReference WriteIntegerU64 { get; }
        
        public MethodReference WriteRawBigEndian32 { get; }
        
        public MethodReference WriteSingle { get; }
        public MethodReference WriteDouble { get; }
        
        public MethodReference WriteString { get; }
        public MethodReference WriteNil { get; set; }
        
        public ReadOnlyDictionary<string, MethodReference> TypeToMethod { get; }


        public MsgPackWriterHelper(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;
            var type = typeof(MsgPackWriter);
            
            TypeRef = _mainModule.ImportReference(type);
            Ctor = _mainModule.ImportReference(type.GetConstructor(new[] {typeof(Allocator)}));

            GetBufferLength = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.GetBufferLength)));
            GetUnsafeBufferPtr = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.GetUnsafeBufferPtr)));
            
            WriteArrayHeader = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteArrayHeader)));
            WriteMapHeader = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteMapHeader)));
            WriteInteger8 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(sbyte)}));
            WriteIntegerU8 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(byte)}));
            WriteInteger16 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(short)}));
            WriteIntegerU16 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(ushort)}));
            WriteInteger32 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(int)}));
            WriteIntegerU32 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(uint)}));
            WriteInteger64 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(long)}));
            WriteIntegerU64 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteInteger), new []{typeof(ulong)}));
            
            WriteSingle = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteSingle)));
            WriteDouble = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteDouble)));
            
            WriteString = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteString)));
            WriteNil =  _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteNil)));
            
            WriteRawBigEndian32 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackWriter.WriteRawBigEndian32)));

            TypeToMethod = GenerateTypeMethodMap();
        }

        private ReadOnlyDictionary<string, MethodReference> GenerateTypeMethodMap()
        {
            Dictionary<string, MethodReference> typeToMethod = new Dictionary<string, MethodReference>();
            
            typeToMethod.Add(typeof(sbyte).FullName, WriteInteger8);
            typeToMethod.Add(typeof(byte).FullName, WriteIntegerU8);
            typeToMethod.Add(typeof(short).FullName, WriteInteger16);
            typeToMethod.Add(typeof(ushort).FullName, WriteIntegerU16);
            typeToMethod.Add(typeof(int).FullName, WriteInteger32);
            typeToMethod.Add(typeof(uint).FullName, WriteIntegerU32);
            typeToMethod.Add(typeof(long).FullName, WriteInteger64);
            typeToMethod.Add(typeof(ulong).FullName, WriteIntegerU64);
            
            typeToMethod.Add(typeof(float).FullName, WriteSingle);
            typeToMethod.Add(typeof(double).FullName, WriteDouble);
            
            typeToMethod.Add(typeof(string).FullName, WriteString);
            
            return new ReadOnlyDictionary<string, MethodReference>(typeToMethod);
        }

        public bool IsSimpleType(TypeReference type) => TypeToMethod.ContainsKey(type.FullName);

        public MethodReference GetMethodForType(TypeReference type)
        {
            return TypeToMethod.TryGetValue(type.FullName, out var methodRef) ? methodRef : null;
        }
    }
}