using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace BetterUnityPlugin
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class HookAttribute : System.Attribute
    {
        public string MethodName;
        public BindingFlags MethodFlags;
        public Type MethodClass;

        public HookAttribute(string methodName, Type methodClass = null, BindingFlags methodFlags = BindingFlags.Default)
        {
            MethodName = methodName;
            MethodFlags = methodFlags;
            MethodClass = methodClass;
        }
    }
}
