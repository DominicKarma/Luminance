namespace Luminance.Common.VerletIntergration
{
    /// <summary>
    /// Configurable settings for verlet simulations.
    /// </summary>
    /// <param name="TileCollision">Whether the segments should collide with tiles.</param>
    /// <param name="SlowInWater">Whether the segments should move slower through water.<br/><b>Does nothing if <paramref name="Gravity"/> is 0.</b></param>
    /// <param name="Gravity">The gravity strength to apply to the segments.</param>
    /// <param name="MaxFallSpeed">The maximum speed the segments should fall due to gravity.</param>
    /// <param name="ConserveEnergy">Whether an additional calcuation should be performed to conserve the energy of the segments.<br/>This results in them bouncing around a lot more before settling down.</param>
    public record VerletSettings(bool TileCollision = false, bool SlowInWater = true, float Gravity = 0.3f, float MaxFallSpeed = 19f, bool ConserveEnergy = false);
}
