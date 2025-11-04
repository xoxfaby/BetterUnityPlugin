using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace BetterUnityPlugin
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    internal class EventAttribute : System.Attribute
    {
        public Type EventClass;
        public string EventName;
        public BindingFlags EventFlags;

        public EventAttribute(Type eventClass, string eventName, BindingFlags eventFlags = BindingFlags.Default)
        {
            EventClass = eventClass;
            EventName = eventName;
            EventFlags = eventFlags;
        }
    }
}
