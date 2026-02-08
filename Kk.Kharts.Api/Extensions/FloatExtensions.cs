namespace Kk.Kharts.Api.Extensions
{
    public static class FloatExtensions
    {
        private const float DefaultTolerance = 0.01f;

        public static bool IsApproximatelyEqualTo(this float a, float b, float tolerance = DefaultTolerance)
        {
            return Math.Abs(a - b) < tolerance;
        }

        //public static bool IsNearlyEqualTo(this float value, float target, float tolerance = 0.1f)
        //{
        //    return Math.Abs(value - target) < tolerance;
        //}

    }
}
