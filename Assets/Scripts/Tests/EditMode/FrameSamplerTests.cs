using System;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class FrameSamplerTests
    {
        [Test]
        public void ASamplingWindowHasToLast()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FrameSampler(0f));
        }

        [Test]
        public void NothingIsReadyBeforeTheWindowCloses()
        {
            FrameSampler sampler = new FrameSampler(1f);

            Assert.That(sampler.Add(0.25f), Is.False);
            Assert.That(sampler.Add(0.25f), Is.False);
            Assert.That(sampler.Fps, Is.Zero);
        }

        [Test]
        public void AClosedWindowReportsWhatItSaw()
        {
            FrameSampler sampler = new FrameSampler(1f);

            for (int frame = 0; frame < 3; frame++)
            {
                sampler.Add(0.25f);
            }

            Assert.That(sampler.Add(0.25f), Is.True);
            Assert.That(sampler.Fps, Is.EqualTo(4f).Within(0.001f));
            Assert.That(sampler.AverageMs, Is.EqualTo(250f).Within(0.001f));
        }

        [Test]
        public void TheWorstFrameOfTheWindowIsKept()
        {
            FrameSampler sampler = new FrameSampler(0.5f);

            sampler.Add(0.1f);
            sampler.Add(0.3f);
            sampler.Add(0.1f);

            Assert.That(sampler.WorstMs, Is.EqualTo(300f).Within(0.001f));
        }

        [Test]
        public void EachWindowStartsAgainFromNothing()
        {
            FrameSampler sampler = new FrameSampler(0.5f);

            sampler.Add(0.5f);
            sampler.Add(0.1f);
            sampler.Add(0.1f);
            sampler.Add(0.1f);
            sampler.Add(0.1f);
            sampler.Add(0.1f);

            Assert.That(sampler.WorstMs, Is.EqualTo(100f).Within(0.001f),
                "The spike from the window before is still being reported.");
        }

        [Test]
        public void AFrameThatTookNoTimeIsIgnored()
        {
            FrameSampler sampler = new FrameSampler(0.5f);

            Assert.That(sampler.Add(0f), Is.False);
            Assert.That(sampler.Add(-1f), Is.False);
        }
    }
}
