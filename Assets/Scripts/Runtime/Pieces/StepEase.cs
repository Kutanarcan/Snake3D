namespace Game.Runtime
{
    public static class StepEase
    {
        public static float Apply(float normalizedTick, float amount)
        {
            float t = Held(normalizedTick);
            float a = Held(amount);

            return t + a * (t * t * (3f - 2f * t) - t);
        }

        private static float Held(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
