using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SystemRandomSourceTests
    {
        [Test]
        public void TheSameSeedReplaysTheSameSequence()
        {
            SystemRandomSource first = new SystemRandomSource(1234);
            SystemRandomSource second = new SystemRandomSource(1234);

            for (int i = 0; i < 50; i++)
            {
                Assert.That(second.Next(100), Is.EqualTo(first.Next(100)));
            }
        }

        [Test]
        public void ReseedingReplaysTheSequenceFromTheStart()
        {
            SystemRandomSource random = new SystemRandomSource(99);
            int[] first = new int[20];
            for (int i = 0; i < first.Length; i++)
            {
                first[i] = random.Next(100);
            }

            random.Reseed(99);

            for (int i = 0; i < first.Length; i++)
            {
                Assert.That(random.Next(100), Is.EqualTo(first[i]));
            }
        }

        [Test]
        public void EveryValueStaysInsideTheRequestedRange()
        {
            SystemRandomSource random = new SystemRandomSource(7);

            for (int i = 0; i < 200; i++)
            {
                Assert.That(random.Next(5), Is.InRange(0, 4));
            }
        }
    }
}
