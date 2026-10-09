using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class EffectCatalogTests
    {
        private EffectCatalog _catalog;
        private EffectView _prefab;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<EffectCatalog>();
            _prefab = NewPrefab();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_prefab.gameObject);
        }

        private static EffectView NewPrefab()
        {
            EffectView view = new GameObject("effect").AddComponent<EffectView>();
            ParticleSystem particles = view.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Callback;

            view.Configure(particles);
            view.gameObject.SetActive(false);

            return view;
        }

        private void Configure(params EffectEntry[] effects)
        {
            _catalog.Configure(effects);
        }

        private EffectEntry Entry(EffectId id)
        {
            return new EffectEntry(id, _prefab, 2, 1f);
        }

        [Test]
        public void ACatalogReportsWhatItHolds()
        {
            Configure(Entry(EffectId.AppleEat), Entry(EffectId.GoldenAppleEat));

            Assert.That(_catalog.Count, Is.EqualTo(2));
            Assert.That(_catalog.At(1).Id, Is.EqualTo(EffectId.GoldenAppleEat));
        }

        [Test]
        public void ValidateAcceptsAnEmptyCatalog()
        {
            Configure();

            Assert.That(_catalog.Validate().IsSuccess, Is.True);
        }

        [Test]
        public void ValidateAcceptsAWellSetUpPrefab()
        {
            Configure(Entry(EffectId.AppleEat));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.True, result.Error);
        }

        [Test]
        public void ValidateRejectsARowWithNoPrefab()
        {
            Configure(new EffectEntry(EffectId.AppleEat, null, 2, 1f));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("AppleEat"));
        }

        [Test]
        public void ValidateRejectsARowThatNamesNoEffect()
        {
            Configure(Entry(EffectId.None));

            Assert.That(_catalog.Validate().IsSuccess, Is.False);
        }

        [Test]
        public void ValidateRejectsTwoRowsForOneEffect()
        {
            Configure(Entry(EffectId.AppleEat), Entry(EffectId.AppleEat));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("two rows"));
        }

        [Test]
        public void ValidateRejectsALoopingPrefab()
        {
            ParticleSystem.MainModule main = _prefab.Particles.main;
            main.loop = true;

            Configure(Entry(EffectId.AppleEat));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("loops"));
        }

        [Test]
        public void ValidateRejectsAPrefabThatNeverCallsBack()
        {
            ParticleSystem.MainModule main = _prefab.Particles.main;
            main.stopAction = ParticleSystemStopAction.None;

            Configure(Entry(EffectId.AppleEat));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Stop Action"));
        }

        [Test]
        public void ValidateRejectsAPrefabThatPlaysOnAwake()
        {
            ParticleSystem.MainModule main = _prefab.Particles.main;
            main.playOnAwake = true;

            Configure(Entry(EffectId.AppleEat));

            Result result = _catalog.Validate();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("plays on awake"));
        }
    }
}
