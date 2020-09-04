using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil;
using TinyMsgPack;
using Unity.Collections;

namespace TinyUtils.JsInterop.CodeGen
{
    public class MsgPackReaderHelper
    {
        private ModuleDefinition _mainModule;
        
        public TypeReference TypeRef { get; }
        public MethodReference Ctor { get; }

        public MethodReference ReadArrayHeader;
        public MethodReference ReadMapHeader;
        
        public MethodReference ReadInt8 { get; }
        public MethodReference ReadIntU8 { get; }
        
        public MethodReference ReadInt16 { get; }
        public MethodReference ReadIntU16 { get; }
        
        public MethodReference ReadInt32 { get; }
        public MethodReference ReadIntU32 { get; }
     
        public MethodReference ReadInt64 { get; }
        public MethodReference ReadIntU64 { get; }
        
        public MethodReference ReadDouble { get; }
        public MethodReference ReadSingle { get; }
        
        public MethodReference ReadString { get; }
        
        public MethodReference TryReadNil { get; }

        public ReadOnlyDictionary<string, MethodReference> TypeToMethod { get; }
        
        public MsgPackReaderHelper(ModuleDefinition mainModule)
        {
            _mainModule = mainModule;

            var type = typeof(MsgPackReader);

            TypeRef = _mainModule.ImportReference(type);
            Ctor = _mainModule.ImportReference(type.GetConstructor(new []{typeof(NativeArray<byte>)}));
            
            ReadArrayHeader = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadArrayHeader)));
            ReadMapHeader = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadMapHeader)));

            ReadInt8 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadInt8)));
            ReadIntU8 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadUInt8)));
            
            ReadInt16 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadInt16)));
            ReadIntU16 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadUInt16)));
            
            ReadInt32 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadInt32)));
            ReadIntU32 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadUInt32)));
            
            ReadInt64 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadInt64)));
            ReadIntU64 = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadUInt64)));
            
            ReadDouble = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadDouble)));
            ReadSingle = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadSingle)));
            
            ReadString = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.ReadString)));
            
            TryReadNil = _mainModule.ImportReference(type.GetMethod(nameof(MsgPackReader.TryReadNil)));
            
            TypeToMethod = GenerateTypeMethodMap();
        }

        private ReadOnlyDictionary<string, MethodReference> GenerateTypeMethodMap()
        {
            Dictionary<string, MethodReference> typeToMethod = new Dictionary<string, MethodReference>();
            
            typeToMethod.Add(typeof(sbyte).FullName, ReadInt8);
            typeToMethod.Add(typeof(byte).FullName, ReadIntU8);
            typeToMethod.Add(typeof(short).FullName, ReadInt16);
            typeToMethod.Add(typeof(ushort).FullName, ReadIntU16);
            typeToMethod.Add(typeof(int).FullName, ReadInt32);
            typeToMethod.Add(typeof(uint).FullName, ReadIntU32);
            typeToMethod.Add(typeof(long).FullName, ReadInt64);
            typeToMethod.Add(typeof(ulong).FullName, ReadIntU64);
            
            typeToMethod.Add(typeof(float).FullName, ReadSingle);
            typeToMethod.Add(typeof(double).FullName, ReadDouble);
            
            typeToMethod.Add(typeof(string).FullName, ReadString);
            
            return new ReadOnlyDictionary<string, MethodReference>(typeToMethod);
        }

        public bool IsSimpleType(TypeReference type) => TypeToMethod.ContainsKey(type.FullName);

        public MethodReference GetMethodForType(TypeReference type)
        {
            return TypeToMethod.TryGetValue(type.FullName, out var methodRef) ? methodRef : null;
        }
    }
}