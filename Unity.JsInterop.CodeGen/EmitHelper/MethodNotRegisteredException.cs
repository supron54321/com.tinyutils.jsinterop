using System;

namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public class MethodNotRegisteredException : Exception
    {
        public MethodNotRegisteredException(string message) : base(message)
        {
            
        }
    }
}