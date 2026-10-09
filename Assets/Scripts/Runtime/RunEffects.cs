using Game.Core;

namespace Game.Runtime
{
    public static class RunEffects
    {
        public static EffectId ForMeal(CellOccupantType meal)
        {
            return meal == CellOccupantType.GoldenApple ? EffectId.GoldenAppleEat : EffectId.AppleEat;
        }
    }
}
