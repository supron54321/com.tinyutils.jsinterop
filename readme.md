# About this project

Tiny JsInterop is a small library which helps with C# <-> JavaScript interoparability in unity engine (Tiny/DOTS runtime and WebGL builds). Idea is simple. Add one attribute and let the system do the job.

# Example

Consider simple task of calling window.prompt from C#. With this library, all you have to do is to declare one external method:

```C#
        [JsFunction("window.prompt")]
        public static extern string Prompt(string message, string defaultValue);
```

That's all! :) Now you can use this function wherever you want (except from job system... yet).

What about Js -> C# calls?

```C#
        [JsCallback("window.unityCallback.CallbackTest")]
        public static string CallbackTest(string messageToUnity)
        {
            ConsoleLog(messageToUnity);
            return "returned from C#!";
        }
```

Once you set **JsCallback** attribute, a method will be available from JS. But how does these attrubites work?

# How does it work

All the magic comes from ILPostProcessor. It's a new and undocumented feature introduced in newer versons of Unity. ILPostProcessor allows us to inject additional IL code after assembly compilation. This library implements its own ILPostProcessor. It looks for **JsCallback** and **JsFunction** attributes, and injects additional code for initialization and parameters/return serialization.

# Installation

Currently the only way to install it is to download both:

* [com.tinyutils.jsinterop](https://github.com/supron54321/com.tinyutils.jsinterop)
* [com.tinyutils.msgpack](https://github.com/supron54321/com.tinyutils.msgpack)

and copy them into "Packages" folder.

# Supported types

Unity DllImport() by default supports only primitive types, strings and arrays of primitives. This library can transfer all serializable complex types except arrays and built-in containers (System.Collections.*). Support for containers is being implemented and will be released soon.

# Exceptions support

JsCallback supports exceptions transfer between C# and JS. JsFunction support will be added in the future. Exceptions do not work in Tiny/DOTS, but they do work in old WebGL build.

# Known issues

* JsCallback does not work in Development mode (NativeList bug)
* Arrays and other containers are not supported yet

# Roadmap

* Containers support
* Reference type support (transfer reference instead of serialized copy)
* Delegates transfer support