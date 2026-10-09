using System;

namespace Game.Core
{
    public readonly struct GoldenAppleRule
    {
        public static readonly GoldenAppleRule None = new GoldenAppleRule(0f, 0f);

        public GoldenAppleRule(float spawnAfterSeconds, float staysForSeconds)
        {
            SpawnAfterSeconds = spawnAfterSeconds;
            StaysForSeconds = staysForSeconds;
        }

        public float SpawnAfterSeconds { get; }
        public float StaysForSeconds { get; }

        public bool IsEnabled
        {
            get { return SpawnAfterSeconds > 0f && StaysForSeconds > 0f; }
        }

        public int SpawnTicks(float tickInterval)
        {
            return Ticks(SpawnAfterSeconds, tickInterval);
        }

        public int StayTicks(float tickInterval)
        {
            return Ticks(StaysForSeconds, tickInterval);
        }

        internal static Result TryRead(GoldenAppleData data, out GoldenAppleRule rule)
        {
            rule = None;

            if (data == null)
            {
                return Result.Success();
            }

            if (data.SpawnAfterSeconds <= 0f || data.StaysForSeconds <= 0f)
            {
                return Result.Failure("The golden apple waits " + data.SpawnAfterSeconds + "s and stays "
                    + data.StaysForSeconds + "s; both must be above zero, or leave the block out.");
            }

            rule = new GoldenAppleRule(data.SpawnAfterSeconds, data.StaysForSeconds);

            return Result.Success();
        }

        private static int Ticks(float seconds, float tickInterval)
        {
            if (seconds <= 0f || tickInterval <= 0f)
            {
                return 0;
            }

            return (int)Math.Ceiling(seconds / (double)tickInterval);
        }
    }
}
