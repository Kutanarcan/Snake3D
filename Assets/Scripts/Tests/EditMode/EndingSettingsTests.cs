using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class EndingSettingsTests
    {
        private EndingSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<EndingSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        private static DeathEffect Death(EndingId id)
        {
            return new DeathEffect(id, SoundId.WallHit, 1f, SoundId.Lose);
        }

        private void Configure(params DeathEffect[] deaths)
        {
            _settings.Configure(new WinEffect(SoundId.Win, 0.5f), deaths);
        }

        [Test]
        public void FindReturnsTheDeathThatNamesTheEnding()
        {
            Configure(Death(EndingId.WallHit), Death(EndingId.SelfBite));

            Assert.That(_settings.Find(EndingId.SelfBite).Id, Is.EqualTo(EndingId.SelfBite));
        }

        [Test]
        public void FindReturnsNothingForAnUnwrittenEnding()
        {
            Configure(Death(EndingId.WallHit));

            Assert.That(_settings.Find(EndingId.SelfBite), Is.Null);
            Assert.That(_settings.Find(EndingId.None), Is.Null);
        }

        [Test]
        public void ValidateAcceptsSettingsThatWriteOnlySomeEndings()
        {
            Configure(Death(EndingId.WallHit));

            Assert.That(_settings.Validate().IsSuccess, Is.True);
        }

        [Test]
        public void ValidateRejectsSettingsWithNoWin()
        {
            _settings.Configure(null, new DeathEffect[0]);

            Result result = _settings.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("win"));
        }

        [Test]
        public void ValidateRejectsADeathThatNamesNoEnding()
        {
            Configure(Death(EndingId.None));

            Assert.That(_settings.Validate().IsSuccess, Is.False);
        }

        [Test]
        public void ValidateRejectsTwoDeathsForOneEnding()
        {
            Configure(Death(EndingId.WallHit), Death(EndingId.WallHit));

            Result result = _settings.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("WallHit"));
        }
    }
}
