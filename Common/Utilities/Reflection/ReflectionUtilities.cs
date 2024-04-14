using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Binding flags that account for all access/local membership status.
        /// </summary>
        public static readonly BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        
        /// <summary>
        ///     Loads all instances of a given mod type (such as <see cref="ModNPC"/>) that have a specific interface attribute.<br></br>
        ///     This method is useful for handling autoloading on multi-attributed pieces of content.
        /// </summary>
        /// <param name="mod">The mod to search in.</param>
        /// <param name="queryCondition">A secondary query condition to apply when collecting interfaces. By default this doesn't affect output results.</param>
        public static IEnumerable<TModType> LoadInterfacesFromContent<TModType, TInterfaceType>(this Mod mod, Func<TModType, bool> queryCondition = null) where TModType : class, ILoadable
        {
            var contentInterfaces = mod.GetContent().Where(c =>
            {
                return c is TModType t and TInterfaceType && (queryCondition?.Invoke(t) ?? true);
            }).Select(c => c as TModType);

            return contentInterfaces;
        }

        /// <summary>
        /// Retrieves all types which derive from a specific type in a given assembly.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="assemblyToSearch">The assembly to search.</param>
        public static IEnumerable<Type> GetEveryTypeDerivedFrom(Type baseType, Assembly assemblyToSearch) =>
            AssemblyManager.GetLoadableTypes(assemblyToSearch).Where(type => !type.IsAbstract && !type.ContainsGenericParameters)
            .Where(type => type.IsAssignableTo(baseType))
            .Where(type =>
            {
                bool derivedHasConstructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
                bool baseHasHasConstructor = type.BaseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
                return derivedHasConstructor || baseHasHasConstructor;
            });
    }
}
