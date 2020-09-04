using System;

namespace TinyUtils.JsInterop
{
    public class JavaScriptException : Exception
    {
        public override string StackTrace { get; }

        public JavaScriptException(JsError error) : base($"JavaScript exception {error.Name}: {error.Message}")
        {
            StackTrace = error.Stack;
        }
    }
}