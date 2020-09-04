using System;

namespace TinyUtils.JsInterop
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PreserveAttribute : Attribute {}
    [AttributeUsage(AttributeTargets.Method)]
    public class MonoPInvokeCallbackAttribute : Attribute {}
}