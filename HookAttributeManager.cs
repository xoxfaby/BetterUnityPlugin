using System;
using System.Collections.Generic;
using System.Text;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using MonoMod.Cil;
using MonoMod.Utils;
using BetterUnityPlugin;

namespace BetterUnityPlugin
{
    internal class HookAttributeManager<T>
    {
        private static List<(MethodInfo methodFrom, MethodInfo methodTo)> hookSignatures = new List<(MethodInfo, MethodInfo)>();
        private static List<(MethodInfo methodFrom, MonoMod.Cil.ILContext.Manipulator ILHookMethod)> ILHookMethods = new List<(MethodInfo, MonoMod.Cil.ILContext.Manipulator)>();
        private static List<(EventInfo eventInfo, Delegate eventHandler)> Events = new List<(EventInfo, Delegate)>();
        private static List<Hook> hooks = new List<Hook>();
        private static List<ILHook> ILHooks = new List<ILHook>();
        private static bool enabled = false;

        private static readonly BindingFlags anyFlags =
                 BindingFlags.Public |
                 BindingFlags.NonPublic |
                 BindingFlags.Instance |
                 BindingFlags.Static;

        public HookAttributeManager()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            foreach (var method in assembly.GetTypes().SelectMany(x => x.GetMethods(anyFlags)))
            {
                foreach (var hookAttribute in method.GetCustomAttributes<HookAttribute>())
                {
                    Type type = hookAttribute.MethodClass;
                    Type[] types = null;

                    try
                    {
                        if (type == null)
                        {
                            type = method.GetParameters()[0].ParameterType.GenericTypeArguments[0];
                        }
                        types = method.GetParameters()[0].ParameterType.GenericTypeArguments;
                    }
                    catch
                    {
                        UnityEngine.Debug.LogError($"Could not hook method {hookAttribute.MethodName}, type not found.");

                        continue;
                    }
                    MethodInfo methodFrom = FindMethod(type, hookAttribute.MethodName, types, hookAttribute.MethodFlags);
                    if (methodFrom == null)
                    {
                        UnityEngine.Debug.LogError($"Could not hook method {hookAttribute.MethodName} of {type}, method not found.");
                        continue;
                    }
                    hookSignatures.Add((methodFrom, method));
                    if (enabled) hooks.Add(new Hook(methodFrom, method));
                }
                foreach (var iLHookAttribute in method.GetCustomAttributes<ILHookAttribute>())
                {
                    Type type = iLHookAttribute.MethodClass;
                    Type[] types = iLHookAttribute.MethodParams;
                    MethodInfo methodFrom = FindMethod(type, iLHookAttribute.MethodName, types, iLHookAttribute.MethodFlags);
                    if (methodFrom == null)
                    {
                        UnityEngine.Debug.LogError($"Could not hook method {iLHookAttribute.MethodName} of {type}, method not found.");
                        continue;
                    }
                    ILContext.Manipulator manipulator = method.CreateDelegate<ILContext.Manipulator>();
                    ILHookMethods.Add((methodFrom, manipulator));
                    if (enabled) ILHooks.Add(new ILHook(methodFrom, manipulator));
                }
                foreach (var eventAttribute in method.GetCustomAttributes<EventAttribute>())
                {
                    EventInfo eventInfo;
                    if(eventAttribute.EventFlags == BindingFlags.Default)
                    {
                        eventInfo = eventAttribute.EventClass.GetEvent(eventAttribute.EventName, anyFlags);
                    }
                    else
                    {
                        eventInfo = eventAttribute.EventClass.GetEvent(eventAttribute.EventName, eventAttribute.EventFlags);
                    }
                    if (eventInfo == null)
                    {
                        UnityEngine.Debug.LogError($"Could not subscribe to event {eventAttribute.EventName} of {eventAttribute.EventClass}, event not found.");
                        continue;
                    }
                    Delegate eventHandler;
                    try
                    {
                        eventHandler = method.CreateDelegate(eventInfo.EventHandlerType);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Could not subscribe to event {eventAttribute.EventName} of {eventAttribute.EventClass}, failed to create eventhandler delegate:\n {ex}");
                        continue;
                    }
                    Events.Add((eventInfo, eventHandler));
                }
            }
        }

        internal void Enable()
        {
            enabled = true;
            foreach (var hook in hookSignatures)
            {
                hooks.Add(new Hook(hook.methodFrom, hook.methodTo));
            }
            foreach (var hook in ILHookMethods)
            {
                ILHooks.Add(new ILHook(hook.methodFrom, hook.ILHookMethod));
            }
            foreach (var _event in Events)
            {
                _event.eventInfo.AddEventHandler(null, _event.eventHandler);
            }
        }

        internal void Disable()
        {
            enabled = false;
            foreach (var hook in hooks)
            {
                hook.Dispose();
            }
            foreach (var hook in ILHooks)
            {
                hook.Dispose();
            }
            foreach (var _event in Events)
            {
                _event.eventInfo.RemoveEventHandler(null, _event.eventHandler);
            }
        }


        public static MethodInfo FindMethod(Type type, string methodName, Type[] types, BindingFlags bindings = BindingFlags.Default)
        {
            MethodInfo methodFrom;
            if (bindings != BindingFlags.Default)
            {
                methodFrom = type.GetMethod(methodName, bindings, null, types, null)
                    ?? type.GetMethod(methodName, bindings);
            }
            else
            {
                methodFrom = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, types, null)
                    ?? type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, types, null)
                    ?? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, types, null)
                    ?? type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static, null, types, null)
                    ?? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
                    ?? type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                    ?? type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            }
            return methodFrom;
        }
        public static Type[] getParameterTypes(MethodInfo methodInfo)
        {
            List<Type> types = new List<Type>();
            var parameters = methodInfo.GetParameters();
            for (int i = 1; i < parameters.Length; i++)
            {
                types.Add(parameters[i].ParameterType);
            }
            return types.ToArray();
        }
    }
}
