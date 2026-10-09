using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class TickTimerTests
    {
        private static int DrainTicks(TickTimer timer)
        {
            int ticks = 0;
            while (timer.TryConsumeTick())
            {
                ticks++;
            }

            return ticks;
        }

        [Test]
        public void Interval_RejectsNonPositiveValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TickTimer(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TickTimer(0.2f).Interval = -1f);
        }

        [Test]
        public void NoTickIsProducedBeforeTheIntervalElapses()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.19f);

            Assert.That(DrainTicks(timer), Is.EqualTo(0));
        }

        [Test]
        public void OneTickIsProducedWhenTheIntervalElapses()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.2f);

            Assert.That(DrainTicks(timer), Is.EqualTo(1));
        }

        [Test]
        public void ALongFrameProducesEveryTickItContains()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.5f);

            Assert.That(DrainTicks(timer), Is.EqualTo(2), "0.5s at a 0.2s interval is two whole ticks.");
        }

        [Test]
        public void RemainderCarriesOverIntoTheNextFrame()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.15f);
            DrainTicks(timer);
            timer.Tick(0.06f);

            Assert.That(DrainTicks(timer), Is.EqualTo(1), "0.15s + 0.06s crosses the interval.");
        }

        [Test]
        public void AVeryLongStallIsClampedInsteadOfReplayingEveryMissedTick()
        {
            // 0.25 is exactly representable, so the clamped accumulator drains to zero
            // without the test resting on float rounding.
            TickTimer timer = new TickTimer(0.25f);

            timer.Tick(30f);

            Assert.That(DrainTicks(timer), Is.EqualTo(4), "Catch-up is capped so the snake cannot teleport after a stall.");
        }

        [Test]
        public void Alpha_ReportsProgressTowardsTheNextTick()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.05f);
            DrainTicks(timer);

            Assert.That(timer.NormalizedTick, Is.EqualTo(0.25f).Within(1e-5f));
        }

        [Test]
        public void Alpha_RestartsFromNearZeroAfterATickIsConsumed()
        {
            TickTimer timer = new TickTimer(0.2f);

            timer.Tick(0.2f);
            DrainTicks(timer);

            Assert.That(timer.NormalizedTick, Is.EqualTo(0f).Within(1e-5f));
        }

        [Test]
        public void Reset_DropsBufferedTime()
        {
            TickTimer timer = new TickTimer(0.2f);
            timer.Tick(0.19f);

            timer.Reset();
            timer.Tick(0.05f);

            Assert.That(DrainTicks(timer), Is.EqualTo(0));
            Assert.That(timer.NormalizedTick, Is.EqualTo(0.25f).Within(1e-5f));
        }

        [Test]
        public void ChangingTheIntervalTakesEffectImmediately()
        {
            TickTimer timer = new TickTimer(0.5f);
            timer.Tick(0.3f);

            Assert.That(DrainTicks(timer), Is.EqualTo(0));

            timer.Interval = 0.2f;

            Assert.That(DrainTicks(timer), Is.EqualTo(1), "The already buffered 0.3s covers one 0.2s tick.");
        }
    }
}
