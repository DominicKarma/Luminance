using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace KarmaLibrary.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Loads all instances of a given mod type (such as <see cref="ModNPC"/>) that have a specific interface attribute (such as <see cref="IBossChecklistSupport"/>).<br></br>
        /// This method is useful for handling autoloading on multi-attributed pieces of content.
        /// </summary>
        /// <param name="mod">The mod to search in.</param>
        /// <param name="queryCondition">A secondary query condition to apply when collecting interfaces. By default this doesn't affect output results.</param>
        public static IEnumerable<ModType> LoadInterfacesFromContent<ModType, InterfaceType>(this Mod mod, Func<ModType, bool> queryCondition = null) where ModType : class, ILoadable
        {
            var contentInterfaces = mod.GetContent().Where(c =>
            {
                return c is ModType t and InterfaceType && (queryCondition?.Invoke(t) ?? true);
            }).Select(c => c as ModType);

            return contentInterfaces;
        }
    }
}
