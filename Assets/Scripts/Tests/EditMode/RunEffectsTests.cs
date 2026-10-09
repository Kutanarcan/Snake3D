using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class RunEffectsTests
    {
        [Test]
        public void TheTwoApplesBurstDifferently()
        {
            Assert.That(RunEffects.ForMeal(CellOccupantType.Food), Is.EqualTo(EffectId.AppleEat));
            Assert.That(RunEffects.ForMeal(CellOccupantType.GoldenApple), Is.EqualTo(EffectId.GoldenAppleEat));
        }
    }
}
