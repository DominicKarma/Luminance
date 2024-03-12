using System;
using System.Linq;
using System.Reflection;

namespace KarmaLibrary.Common.Tools.Reflection
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AutomatedMethodInvokeAttribute : Attribute
    {
        public AutomatedMethodInvokeAttribute() { }

        /// <summary>
        /// Invokes all methods marked with a <see cref="AutomatedMethodInvokeAttribute"/> for a given object.
        /// </summary>
        /// <param name="instance">The object instance to check methods for.</param>
        public static void InvokeWithAttribute(object instance)
        {
            MethodInfo[] methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).
                // Ignore methods without this attribute or any variable parameters to account for.
                Where(m => m.GetCustomAttributes<AutomatedMethodInvokeAttribute>().Any() && !m.GetParameters().Any()).
                ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                // Invoke the method. If it's instanced, supply the instance. Otherwise, supply nothing.
                method.Invoke(method.IsStatic ? null : instance, []);
            }
        }
    }
}
