using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class RunSoundsTests
    {
        [Test]
        public void TheTwoApplesAreEatenDifferently()
        {
            Assert.That(RunSounds.ForMeal(CellOccupantType.Food), Is.EqualTo(SoundId.AppleEat));
            Assert.That(RunSounds.ForMeal(CellOccupantType.GoldenApple), Is.EqualTo(SoundId.GoldenAppleEat));
        }

        [Test]
        public void TheTwoApplesAnnounceThemselvesDifferently()
        {
            Assert.That(RunSounds.ForArrival(false), Is.EqualTo(SoundId.AppleAppear));
            Assert.That(RunSounds.ForArrival(true), Is.EqualTo(SoundId.GoldenAppleAppear));
        }
    }
}
