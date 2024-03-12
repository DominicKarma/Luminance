namespace KarmaLibrary.Common.Tools.Easings
{
    public class LinearEasing : EasingCurve
    {
        public static readonly LinearEasing Default = new();

        public LinearEasing()
        {
            InCurve = new(interpolant =>
            {
                return interpolant;
            });
            OutCurve = new(interpolant =>
            {
                return interpolant;
            });
            InOutCurve = new(interpolant =>
            {
                return interpolant;
            });
        }
    }
}
