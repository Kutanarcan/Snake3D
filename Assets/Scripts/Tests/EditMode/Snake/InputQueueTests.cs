using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class InputQueueTests
    {
        private InputQueue _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new InputQueue();
        }

        [Test]
        public void AFreshQueueHoldsNothing()
        {
            Assert.That(_queue.IsEmpty, Is.True);
            Assert.That(_queue.IsFull, Is.False);
            Assert.That(_queue.Count, Is.Zero);
        }

        [Test]
        public void WhatGoesInComesOutInOrder()
        {
            _queue.TryEnqueue(Direction.Right);
            _queue.TryEnqueue(Direction.Down);

            Direction first;
            Direction second;

            Assert.That(_queue.TryDequeue(out first), Is.True);
            Assert.That(_queue.TryDequeue(out second), Is.True);

            Assert.That(first, Is.EqualTo(Direction.Right));
            Assert.That(second, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void TheEndsAreTheOldestAndTheNewest()
        {
            _queue.TryEnqueue(Direction.Right);
            _queue.TryEnqueue(Direction.Down);

            Assert.That(_queue.Front, Is.EqualTo(Direction.Right));
            Assert.That(_queue.Back, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void AFullQueueTurnsDownWhatItCannotHold()
        {
            for (int i = 0; i < InputQueue.Depth; i++)
            {
                Assert.That(_queue.TryEnqueue(Direction.Right), Is.True);
            }

            Assert.That(_queue.IsFull, Is.True);
            Assert.That(_queue.TryEnqueue(Direction.Down), Is.False);
            Assert.That(_queue.Count, Is.EqualTo(InputQueue.Depth));
        }

        [Test]
        public void TakingOneMakesRoomForAnother()
        {
            _queue.TryEnqueue(Direction.Right);
            _queue.TryEnqueue(Direction.Down);

            Direction taken;
            _queue.TryDequeue(out taken);

            Assert.That(_queue.TryEnqueue(Direction.Left), Is.True);
            Assert.That(_queue.Front, Is.EqualTo(Direction.Down));
            Assert.That(_queue.Back, Is.EqualTo(Direction.Left));
        }

        [Test]
        public void AnEmptyQueueHasNothingToGive()
        {
            Direction taken;

            Assert.That(_queue.TryDequeue(out taken), Is.False);
        }

        [Test]
        public void ClearingDropsEverythingWaiting()
        {
            _queue.TryEnqueue(Direction.Right);
            _queue.TryEnqueue(Direction.Down);

            _queue.Clear();

            Assert.That(_queue.IsEmpty, Is.True);
        }
    }
}
