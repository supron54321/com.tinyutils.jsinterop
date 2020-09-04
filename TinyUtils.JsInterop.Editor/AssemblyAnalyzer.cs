using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


public class AssemblyAnalyzer 
{
    [MenuItem("JsInterop/Analyze")]
    public static void Analyze()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.LogError(assembly);
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Debug.Log(loaderException);
                }
            }
        }
    }
}
