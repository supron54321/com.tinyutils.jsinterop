using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using OpCode = Mono.Cecil.Cil.OpCode;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using TinyUtils.JsInterop.CodeGen.Helper;

namespace TinyUtils.JsInterop.CodeGen
{
    public class SerializationClassCodegen
    {
        private ModuleDefinition _mainModule;
        private TypeHelpersCollection _typeHelper;
        private TypeDefinition _serializerClass;

        Dictionary<TypeReference, MethodReference> _serializationMethods =
            new Dictionary<TypeReference, MethodReference>();

        Dictionary<TypeReference, MethodReference> _deserializationMethods =
            new Dictionary<TypeReference, MethodReference>();

        public SerializationClassCodegen(ModuleDefinition mainModule, TypeHelpersCollection typeHelper)
        {
            _mainModule = mainModule;
            _typeHelper = typeHelper;
            _serializerClass = new TypeDefinition("JsInterop.Codegen", "Serialization",
                TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed,
                _mainModule.TypeSystem.Object);
            mainModule.Types.Add(_serializerClass);
        }

        public MethodReference GetTypeDeserializationMethod(TypeReference type)
        {
            if (_deserializationMethods.TryGetValue(type, out var methodReference)) return methodReference;
            return GenerateDeserializeMethodForType(type);
        }

        private MethodReference GenerateDeserializeMethodForType(TypeReference type)
        {
            var method = new MethodDefinition($"Deserialize{type.Name}",
                MethodAttributes.Public | MethodAttributes.Static, type);
            _serializerClass.Methods.Add(method);
            _deserializationMethods.Add(type, method);

            var readerParam =
                new ParameterDefinition("reader", ParameterAttributes.None, _typeHelper.MsgPackReader.TypeRef);

            method.Parameters.Add(readerParam);

            var ilProcessor = method.Body.GetILProcessor();
            BuildDeserializeMethodBody(ilProcessor, type, readerParam);

            return method;
        }

        private void BuildDeserializeMethodBody(ILProcessor ilProcessor, TypeReference type,
            ParameterDefinition readerParam)
        {
            if (_typeHelper.MsgPackReader.IsSimpleType(type))
            {
                var deserializeMethod = _typeHelper.MsgPackReader.GetMethodForType(type);
                ilProcessor.CallMethod(readerParam, deserializeMethod);
                ilProcessor.Emit(OpCodes.Ret);
            }
            else
            {
                var resolved = type.Resolve();
                if (type.IsPrimitive)
                    throw new NotImplementedException($"Unsupported primitive {type.FullName}");
                if (type.IsArray)
                    throw new NotImplementedException("Arrays are not supported");
                if (type.HasGenericParameters)
                    throw new NotImplementedException("Generic types are not supported");
                else
                {
                    if (!resolved.HasFields)
                        throw new NotImplementedException("Empty type is not supported");
                    BuildDeserializeComplexClassBody(ilProcessor, readerParam, resolved);
                }
            }
        }

        private void BuildDeserializeComplexClassBody(ILProcessor ilProcessor,
            ParameterDefinition readerParam, TypeDefinition resolved)
        {
            var resultVar = ilProcessor.AddVariable(resolved);
            var fieldCountVar = ilProcessor.AddVariable(_mainModule.TypeSystem.Int32);

            Instruction retInstr = Instruction.Create(OpCodes.Ret);
            if (resolved.IsValueType)
            {
                ilProcessor.Emit(OpCodes.Ldloca_S, resultVar);
                ilProcessor.Emit(OpCodes.Initobj, resolved);

                var mapTempVar = ilProcessor.AddVariable(_mainModule.TypeSystem.Boolean);
                ilProcessor.Emit(OpCodes.Ldarg, readerParam);
                ilProcessor.Emit(OpCodes.Ldloca_S, fieldCountVar);
                ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackReader.ReadMapHeader);
                ilProcessor.Emit(OpCodes.Stloc, mapTempVar);

                EmitFieldsAssignmentLoop(ilProcessor, resultVar, fieldCountVar, readerParam, resolved);
            }
            else
            {
                var ifTempVar = ilProcessor.AddVariable(_mainModule.TypeSystem.Boolean);
                ilProcessor.CallMethod(readerParam, _typeHelper.MsgPackReader.TryReadNil).Store(ifTempVar);
                ilProcessor.If(ifTempVar).True().Then(() =>
                {
                    ilProcessor.Emit(OpCodes.Ldnull);
                    ilProcessor.Emit(OpCodes.Stloc, resultVar);
                }).Else(() =>
                {
                    var ctor = resolved.GetConstructors().FirstOrDefault(ctr => ctr.Parameters.Count == 0);
                    if (ctor == null)
                        throw new InvalidOperationException($"Type {resolved} does not have default constructor");
                    ilProcessor.Emit(OpCodes.Newobj, ctor);
                    ilProcessor.Emit(OpCodes.Stloc, resultVar);

                    var mapTempVar = ilProcessor.AddVariable(_mainModule.TypeSystem.Boolean);
                    ilProcessor.Emit(OpCodes.Ldarg, readerParam);
                    ilProcessor.Emit(OpCodes.Ldloca_S, fieldCountVar);
                    ilProcessor.Emit(OpCodes.Call, _typeHelper.MsgPackReader.ReadMapHeader);
                    ilProcessor.Emit(OpCodes.Stloc, mapTempVar);

                    //ilProcessor.CallMethod(readerParam, _typeHelper.MsgPackReader.ReadMapHeader, fieldCountVar).Store(mapTempVar);

                    EmitFieldsAssignmentLoop(ilProcessor, resultVar, fieldCountVar, readerParam, resolved);
                });
            }

            ilProcessor.Emit(OpCodes.Ldloc, resultVar);
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitFieldsAssignmentLoop(ILProcessor ilProcessor, VariableDefinition resultVar,
            VariableDefinition fieldCountVar, ParameterDefinition readerParam, TypeDefinition resolved)
        {
            var iterationVar = ilProcessor.AddVariable(_mainModule.TypeSystem.Int32);
            var fieldNameVar = ilProcessor.AddVariable(_mainModule.TypeSystem.String);

            var tempstr1 = ilProcessor.AddVariable(_mainModule.TypeSystem.String);
            var switchFieldVar = ilProcessor.AddVariable(_mainModule.TypeSystem.String);

            ilProcessor.For(0, fieldCountVar, 1).Then(() =>
            {
                ilProcessor.CallMethod(readerParam, _typeHelper.MsgPackReader.ReadString).Store(fieldNameVar);
                var switchStatement = ilProcessor.Switch(fieldNameVar);
                foreach (var field in resolved.Fields)
                {
                    switchStatement.Case(field.Name, () =>
                    {
                        if (resolved.IsValueType)
                            ilProcessor.Emit(OpCodes.Ldloca, resultVar);
                        else
                            ilProcessor.Emit(OpCodes.Ldloc, resultVar);
                        var deserializationMethod = GetTypeDeserializationMethod(field.FieldType);
                        ilProcessor.Emit(OpCodes.Ldarg, readerParam);
                        ilProcessor.Emit(OpCodes.Call, deserializationMethod);
                        ilProcessor.Emit(OpCodes.Stfld, field);
                    });
                }
            });

        }

        public MethodReference GetTypeSerializeMethod(TypeReference type)
        {
            if (_serializationMethods.TryGetValue(type, out var methodReference)) return methodReference;
            return GenerateSerializeMethodForType(type);
        }

        private MethodReference GenerateSerializeMethodForType(TypeReference type)
        {
            var method = new MethodDefinition($"Serialize{type.Name}",
                MethodAttributes.Public | MethodAttributes.Static, _mainModule.TypeSystem.Void);
            _serializerClass.Methods.Add(method);
            _serializationMethods.Add(type, method);


            var ilProcessor = method.Body.GetILProcessor();

            var valueParam = ilProcessor.AddParameter("value", ParameterAttributes.None, type);
            var writerParam = ilProcessor.AddParameter("writer", ParameterAttributes.None, _typeHelper.MsgPackWriter.TypeRef);

            BuildSerializeMethodBody(ilProcessor, valueParam, writerParam);

            return method;
        }

        private void BuildSerializeMethodBody(ILProcessor ilProcessor, ParameterDefinition valueParam,
            ParameterDefinition writerParam)
        {
            if (_typeHelper.MsgPackWriter.IsSimpleType(valueParam.ParameterType))
            {
                var serializeMethod = _typeHelper.MsgPackWriter.GetMethodForType(valueParam.ParameterType);
                ilProcessor.CallMethod(writerParam, serializeMethod, valueParam);
                ilProcessor.Emit(OpCodes.Ret);
            }
            else
            {
                var resolved = valueParam.ParameterType.Resolve();
                if (valueParam.ParameterType.IsPrimitive)
                    throw new NotImplementedException($"Unsupported primitive {valueParam.ParameterType.FullName}");
                if (valueParam.ParameterType.IsArray)
                    throw new NotImplementedException("Arrays are not supported");
                if (valueParam.ParameterType.HasGenericParameters)
                    throw new NotImplementedException("Generic types are not supported");
                else
                {
                    if (!resolved.HasFields)
                        throw new NotImplementedException("Empty type is not supported");
                    BuildSerializeComplexClassBody(ilProcessor, valueParam, writerParam, resolved);
                }
            }
        }

        private void BuildSerializeComplexClassBody(ILProcessor ilProcessor, ParameterDefinition valueParam,
            ParameterDefinition writerParam, TypeDefinition resolved)
        {
            if (!resolved.IsValueType)
            {
                ilProcessor.If(valueParam).Null().Then(() =>
                {
                    ilProcessor.CallMethod(writerParam, _typeHelper.MsgPackWriter.WriteNil);
                }).Else(() =>
                {
                    EmitSerializationCode(ilProcessor, valueParam, writerParam, resolved);
                });
            }
            else
            {
                EmitSerializationCode(ilProcessor, valueParam, writerParam, resolved);
            }
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitSerializationCode(ILProcessor ilProcessor, ParameterDefinition valueParam, ParameterDefinition writerParam, TypeDefinition resolved)
        {
            ilProcessor.CallMethod(writerParam, _typeHelper.MsgPackWriter.WriteMapHeader, resolved.Fields.Count);
            foreach (var field in resolved.Fields)
            {
                ilProcessor.CallMethod(writerParam, _typeHelper.MsgPackWriter.WriteString, field.Name);
                ilProcessor.CallStatic(GetTypeSerializeMethod(field.FieldType), (valueParam, field), writerParam);
            }
        }
    }
}