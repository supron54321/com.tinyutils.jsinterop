using System;

namespace TinyUtils.JsInterop
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JsCallback : Attribute
    {
        public JsCallback(string path)
        {
            
        }
    }
}