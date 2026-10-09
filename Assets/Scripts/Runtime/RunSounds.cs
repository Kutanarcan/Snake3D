using Game.Core;

namespace Game.Runtime
{
    public static class RunSounds
    {
        public static SoundId ForMeal(CellOccupantType meal)
        {
            return meal == CellOccupantType.GoldenApple ? SoundId.GoldenAppleEat : SoundId.AppleEat;
        }

        public static SoundId ForArrival(bool isGolden)
        {
            return isGolden ? SoundId.GoldenAppleAppear : SoundId.AppleAppear;
        }
    }
}
