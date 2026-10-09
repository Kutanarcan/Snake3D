using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SnakeBodyTests
    {
        [Test]
        public void Reset_LaysThePartsOutBehindTheHead()
        {
            SnakeBody body = new SnakeBody(16);

            body.Reset(new GridPosition(4, 2), Direction.Up, 3);

            Assert.That(body.Length, Is.EqualTo(3));
            Assert.That(body[0], Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(body[1], Is.EqualTo(new GridPosition(4, 1)));
            Assert.That(body[2], Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(body.Head, Is.EqualTo(body[0]));
            Assert.That(body.Tail, Is.EqualTo(body[2]));
        }

        [Test]
        public void Reset_RejectsLengthsOutsideCapacity()
        {
            SnakeBody body = new SnakeBody(4);

            Assert.Throws<ArgumentOutOfRangeException>(() => body.Reset(new GridPosition(0, 0), Direction.Up, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => body.Reset(new GridPosition(0, 0), Direction.Up, 5));
        }

        [Test]
        public void Advance_WithoutGrowing_KeepsLengthAndDropsTheTail()
        {
            SnakeBody body = new SnakeBody(16);
            body.Reset(new GridPosition(2, 2), Direction.Right, 3);

            body.Advance(new GridPosition(3, 2), false);

            Assert.That(body.Length, Is.EqualTo(3));
            Assert.That(body[0], Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(body[1], Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(body[2], Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(body.IndexOf(new GridPosition(0, 2)), Is.EqualTo(-1), "The old tail cell should have been released.");
        }

        [Test]
        public void Advance_WithGrowing_KeepsTheTailInPlace()
        {
            SnakeBody body = new SnakeBody(16);
            body.Reset(new GridPosition(2, 2), Direction.Right, 3);

            body.Advance(new GridPosition(3, 2), true);

            Assert.That(body.Length, Is.EqualTo(4));
            Assert.That(body[0], Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(body.Tail, Is.EqualTo(new GridPosition(0, 2)));
        }

        [Test]
        public void Advance_AtCapacity_Throws()
        {
            SnakeBody body = new SnakeBody(2);
            body.Reset(new GridPosition(1, 0), Direction.Right, 2);

            Assert.Throws<InvalidOperationException>(() => body.Advance(new GridPosition(2, 0), true));
        }

        [Test]
        public void Advance_WrapsAroundTheRingBufferWithoutCorruptingOrder()
        {
            SnakeBody body = new SnakeBody(3);
            body.Reset(new GridPosition(0, 0), Direction.Right, 2);

            for (int x = 1; x <= 10; x++)
            {
                body.Advance(new GridPosition(x, 0), false);
            }

            Assert.That(body.Length, Is.EqualTo(2));
            Assert.That(body[0], Is.EqualTo(new GridPosition(10, 0)));
            Assert.That(body[1], Is.EqualTo(new GridPosition(9, 0)));
        }

        [Test]
        public void Indexer_RejectsIndexesOutsideTheCurrentLength()
        {
            SnakeBody body = new SnakeBody(8);
            body.Reset(new GridPosition(0, 0), Direction.Up, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => { GridPosition unused = body[-1]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { GridPosition unused = body[2]; });
        }

        [Test]
        public void IndexOf_ReturnsThePartStandingOnTheCell()
        {
            SnakeBody body = new SnakeBody(8);
            body.Reset(new GridPosition(3, 3), Direction.Up, 3);

            Assert.That(body.IndexOf(new GridPosition(3, 3)), Is.EqualTo(0));
            Assert.That(body.IndexOf(new GridPosition(3, 1)), Is.EqualTo(2));
            Assert.That(body.IndexOf(new GridPosition(0, 0)), Is.EqualTo(-1));
        }
    }
}
