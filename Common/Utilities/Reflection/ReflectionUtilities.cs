using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

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
    }
}
