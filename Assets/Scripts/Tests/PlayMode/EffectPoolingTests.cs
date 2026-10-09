using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.PlayMode
{
    public sealed class EffectPoolingTests
    {
        private GameRig _rig;
        private GameObject _host;
        private Transform _root;

        [SetUp]
        public void SetUp()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _host = new GameObject("effects");
            _root = _host.transform;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_host);
            _rig.Destroy();
            _rig = null;
        }

        private EffectPool NewPool(int prewarm)
        {
            return new EffectPool(new EffectEntry(EffectId.AppleEat, _rig.EffectPrefab, prewarm, 1f), _root);
        }

        [Test]
        public void APoolIsFilledBeforeAnythingAsksForIt()
        {
            EffectPool pool = NewPool(3);

            Assert.That(pool.IdleCount, Is.EqualTo(3));
            Assert.That(pool.LiveCount, Is.Zero);
        }

        [Test]
        public void PlayingTakesOneOutOfStock()
        {
            EffectPool pool = NewPool(2);

            pool.Play(Vector3.zero);

            Assert.That(pool.IdleCount, Is.EqualTo(1));
            Assert.That(pool.LiveCount, Is.EqualTo(1));
        }

        [Test]
        public void AnEmptyPoolBuildsOneRatherThanDroppingTheEffect()
        {
            EffectPool pool = NewPool(0);

            pool.Play(Vector3.zero);

            Assert.That(pool.LiveCount, Is.EqualTo(1));
        }      

        [Test]
        public void StoppingEverythingPutsItAllBackAtOnce()
        {
            EffectPool pool = NewPool(1);
            pool.Play(Vector3.zero);

            pool.StopAll();

            Assert.That(pool.LiveCount, Is.Zero);
            Assert.That(pool.IdleCount, Is.EqualTo(1));
        }

        [Test]
        public void TheEngineSayingAnEffectStoppedPutsItBack()
        {
            EffectPool pool = NewPool(1);
            pool.Play(Vector3.zero);

            _root.GetComponentInChildren<EffectView>(true).SendMessage("OnParticleSystemStopped");

            Assert.That(pool.LiveCount, Is.Zero);
            Assert.That(pool.IdleCount, Is.EqualTo(1));
        }

        [Test]
        public void APoolLendsTheSameInstanceAgain()
        {
            EffectPool pool = NewPool(1);

            pool.Play(Vector3.zero);
            pool.StopAll();
            pool.Play(Vector3.zero);

            Assert.That(pool.IdleCount, Is.Zero);
            Assert.That(pool.LiveCount, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<EffectView>(true).Length, Is.EqualTo(1),
                "A second instance was built for an effect that was already back in stock.");
        }
    }
}
