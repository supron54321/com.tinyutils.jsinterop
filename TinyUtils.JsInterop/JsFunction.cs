using System;

namespace TinyUtils.JsInterop
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JsFunction : Attribute
    {
        public JsFunction(string path)
        {
            
        }
    }
}