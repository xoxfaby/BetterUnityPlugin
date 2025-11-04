using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace BetterUnityPlugin
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class ILHookAttribute : Attribute
    {
        public Type MethodClass;
        public string MethodName;
        public Type[] MethodParams;
        public BindingFlags MethodFlags;

        public ILHookAttribute(Type methodClass = null, string methodName, Type[] methodParams = null, BindingFlags methodFlags = BindingFlags.Default)
        {
            MethodClass = methodClass;
            MethodName = methodName;
            MethodParams = methodParams;
            MethodFlags = methodFlags;
        }
    }
}
