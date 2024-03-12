using System;

namespace KarmaLibrary.Common.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        /// Gives the <b>real</b> modulo of a divided by a divisor.
        /// This method is necessary because the % operator in C# keeps the sign of the dividend.
        /// </summary>
        public static float Modulo(this float dividend, float divisor)
        {
            return dividend - (float)Math.Floor(dividend / divisor) * divisor;
        }

        /// <summary>
        /// Approximates the derivative of a function at a given point based on a 
        /// </summary>
        /// <param name="fx">The function to take the derivative of.</param>
        /// <param name="x">The value to evaluate the derivative at.</param>
        public static double ApproximateDerivative(this Func<double, double> fx, double x)
        {
            double left = fx(x + 1e-7);
            double right = fx(x - 1e-7);
            return (left - right) * 5e6;
        }

        /// <summary>
        /// Searches for an approximate for a root of a given function.
        /// </summary>
        /// <param name="fx">The function to find the root for.</param>
        /// <param name="initialGuess">The initial guess for what the root could be.</param>
        /// <param name="iterations">The amount of iterations to perform. The higher this is, the more generally accurate the result will be.</param>
        public static double IterativelySearchForRoot(this Func<double, double> fx, double initialGuess, int iterations)
        {
            // This uses the Newton-Raphson method to iteratively get closer and closer to roots of a given function.
            // The exactly formula is as follows:
            // x = x - f(x) / f'(x)
            // In most circumstances repeating the above equation will result in closer and closer approximations to a root.
            // The exact reason as to why this intuitively works can be found at the following video:
            // https://www.youtube.com/watch?v=-RdOwhmqP5s
            double result = initialGuess;
            for (int i = 0; i < iterations; i++)
            {
                double derivative = fx.ApproximateDerivative(result);
                result -= fx(result) / derivative;
            }

            return result;
        }
    }
}
