using Terraria;
using Terraria.Utilities;

namespace Luminance.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Rolls a random 0-1 probability based on a <see cref="UnifiedRandom"/> RNG, and checks whether it fits the criteria of a certain probability.
        /// </summary>
        /// <param name="rng">The random number generator.</param>
        /// <param name="probability">The probability of a success.</param>
        public static bool NextBool(this UnifiedRandom rng, float probability) =>
            rng.NextFloat() < probability;
    }
}
