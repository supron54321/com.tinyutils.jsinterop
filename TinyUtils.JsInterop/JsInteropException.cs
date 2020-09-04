using System;

namespace TinyUtils.JsInterop
{
    public class JsInteropException : InvalidOperationException
    {
        public JsInteropException(string message) : base(message)
        {
            
        }
    }
}