using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class SoundCatalogTests
    {
        private SoundCatalog _catalog;
        private AudioClip _clip;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<SoundCatalog>();
            _clip = AudioClip.Create("clip", 1, 1, 8000, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_clip);
        }

        [Test]
        public void Find_ReturnsTheRowThatNamesTheSound()
        {
            _catalog.Configure(new[]
            {
                new SoundEntry(SoundId.AppleEat, _clip, 0.5f),
                new SoundEntry(SoundId.Lose, _clip, 1f)
            });

            SoundEntry sound = _catalog.Find(SoundId.Lose);

            Assert.That(sound, Is.Not.Null);
            Assert.That(sound.Id, Is.EqualTo(SoundId.Lose));
            Assert.That(sound.Volume, Is.EqualTo(1f));
        }

        [Test]
        public void Find_ReturnsNothingForAnUnauthoredSound()
        {
            _catalog.Configure(new[] { new SoundEntry(SoundId.AppleEat, _clip, 1f) });

            Assert.That(_catalog.Find(SoundId.Win), Is.Null);
        }

        [Test]
        public void Find_ReturnsNothingForNone()
        {
            _catalog.Configure(new[] { new SoundEntry(SoundId.None, _clip, 1f) });

            Assert.That(_catalog.Find(SoundId.None), Is.Null);
        }

        [Test]
        public void Validate_AcceptsACatalogThatAuthorsOnlySomeSounds()
        {
            _catalog.Configure(new[] { new SoundEntry(SoundId.AppleEat, _clip, 1f) });

            Assert.That(_catalog.Validate().IsSuccess, Is.True);
        }

        [Test]
        public void Validate_AcceptsAnEmptyCatalog()
        {
            _catalog.Configure(new SoundEntry[0]);

            Assert.That(_catalog.Validate().IsSuccess, Is.True);
        }

        [Test]
        public void Validate_RejectsARowWithNoClip()
        {
            _catalog.Configure(new[] { new SoundEntry(SoundId.AppleEat, null, 1f) });

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Eat"));
        }

        [Test]
        public void Validate_RejectsARowThatNamesNoSound()
        {
            _catalog.Configure(new[] { new SoundEntry(SoundId.None, _clip, 1f) });

            Assert.That(_catalog.Validate().IsSuccess, Is.False);
        }

        [Test]
        public void Validate_RejectsTwoRowsForTheSameSound()
        {
            _catalog.Configure(new[]
            {
                new SoundEntry(SoundId.AppleEat, _clip, 1f),
                new SoundEntry(SoundId.AppleEat, _clip, 0.5f)
            });

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Eat"));
        }
    }
}
