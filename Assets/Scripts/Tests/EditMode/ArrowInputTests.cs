using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class ArrowInputTests
    {
        private ArrowInput _input;

        [SetUp]
        public void SetUp()
        {
            _input = new ArrowInput();
        }

        private Direction Read()
        {
            Direction direction;
            Assert.That(_input.TryRead(out direction), Is.True, "A tap was expected to be waiting.");

            return direction;
        }

        private void AssertNothingWaiting()
        {
            Direction direction;
            Assert.That(_input.TryRead(out direction), Is.False);
        }

        [Test]
        public void APadThatWasNotTouchedHasNothingToSay()
        {
            AssertNothingWaiting();
        }

        [Test]
        public void ATapWaitsForTheRunToReadIt()
        {
            _input.Press(Direction.Left);

            Assert.That(Read(), Is.EqualTo(Direction.Left));
        }

        [Test]
        public void ATapIsSpentOnce()
        {
            _input.Press(Direction.Down);

            Read();

            AssertNothingWaiting();
        }

        [Test]
        public void TheLastTapWinsWhenTwoLandBeforeTheRunLooks()
        {
            _input.Press(Direction.Left);
            _input.Press(Direction.Up);

            Assert.That(Read(), Is.EqualTo(Direction.Up), "Two taps in one frame is the player changing their mind.");
        }

        [Test]
        public void ClearingDropsATapThatWasStillWaiting()
        {
            _input.Press(Direction.Right);
            _input.Clear();

            AssertNothingWaiting();
        }
    }
}
