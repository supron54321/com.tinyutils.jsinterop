using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public static class AddVariableExtension
    {
        public static VariableDefinition AddVariable(this ILProcessor processor, TypeReference type)
        {
            var ret = new VariableDefinition(type);
            processor.Body.Variables.Add(ret);
            return ret;
        }

        public static ParameterDefinition AddParameter(this ILProcessor processor, string name, ParameterAttributes attributes, TypeReference type)
        {
            var ret = new ParameterDefinition(name, attributes, type);
            processor.Body.Method.Parameters.Add(ret);
            return ret;
        }
    }
}