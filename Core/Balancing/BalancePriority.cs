namespace Luminance.Core.Balancing
{
    /// <summary>
    /// Defines what priority a balance change should have in case of conflicts. Only the change with the highest priority will occur.
    /// </summary>
    public enum BalancePriority
    {
        /// <summary>
        /// Use if the balance change isnt that important, and being overwritten by other mod's balancing is fine.
        /// </summary>
        Low = 2,

        /// <summary>
        /// Use if the balance change should happen, but it isnt a huge deal if it gets overwritten.
        /// </summary>
        Medium = 4,

        /// <summary>
        /// Use if the balance change is quite important, and being overwritten could noticably mess with balance.
        /// </summary>
        High = 6,

        /// <summary>
        /// Use if the balance change is very important, and should only be overwritten in extreme cases.
        /// </summary>
        VeryHigh = 8,

        /// <summary>
        /// Use sparingly if the balance change MUST happen, and should not be overwritten.
        /// </summary>
        ExtremelyHigh = 10
    }
}
