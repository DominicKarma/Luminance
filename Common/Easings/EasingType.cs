namespace KarmaLibrary.Common.Easings
{
    /// <summary>
    ///     An easing mode.
    /// </summary>
    /// <remarks>
    ///     "In" curves start out slowly but gradually reach their end value, similar to an x^2 function.<br></br>
    ///     "Out" curves start out quickly but gradually slow down to reach their end value, similar to an x^0.5 function.<br></br>
    ///     "InOut" curves start out and end gradually, but accelerate near the middle, similar to a smoothstep function.
    /// </remarks>
    public enum EasingType
    {
        In,
        Out,
        InOut
    }
}
