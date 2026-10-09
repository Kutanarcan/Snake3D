using System;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SnakeTrailTests
    {
        // Head at (5,3) facing Right, so the body trails back through (4,3) to (3,3).
        private static Snake NewSnake()
        {
            Snake snake = new Snake(64);
            snake.Reset(new GridPosition(5, 3), Direction.Right, 3);

            return snake;
        }

        private static SnakeTrail Watching(Snake snake)
        {
            SnakeTrail trail = new SnakeTrail(snake);
            trail.Restart();
            trail.Sync();

            return trail;
        }

        private static void AssertStandingStill(SnakeTrail trail)
        {
            for (int i = 0; i < trail.Length; i++)
            {
                Assert.That(trail.From(i), Is.EqualTo(trail.To(i)), "Part " + i + " is being dragged from somewhere.");
            }
        }

        [Test]
        public void BeforeTheFirstStepEveryPartStandsStill()
        {
            AssertStandingStill(Watching(NewSnake()));
        }

        [Test]
        public void EachPartComesOutOfTheCellTheOneAheadOfItLeft()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(false);
            trail.Sync();

            Assert.That(trail.To(0), Is.EqualTo(new GridPosition(6, 3)));
            Assert.That(trail.From(0), Is.EqualTo(new GridPosition(5, 3)));
            Assert.That(trail.From(1), Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(trail.From(1), Is.EqualTo(trail.To(2)), "The body is its own history, one index over.");
        }

        [Test]
        public void TheTailComesOutOfTheCellItLeftBehind()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(false);
            trail.Sync();

            Assert.That(trail.From(2), Is.EqualTo(new GridPosition(3, 3)),
                "The only cell that falls off the body is the one worth remembering.");
        }

        [Test]
        public void GrowingLeavesTheTailWhereItAlreadyIs()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(true);
            trail.Sync();

            Assert.That(trail.Length, Is.EqualTo(4));
            Assert.That(trail.To(3), Is.EqualTo(new GridPosition(3, 3)));
            Assert.That(trail.From(3), Is.EqualTo(trail.To(3)), "Nothing was vacated, so the new part must not slide.");
        }

        [Test]
        public void TheStepAfterGrowingMovesTheTailAgain()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(true);
            trail.Sync();

            snake.Step(false);
            trail.Sync();

            Assert.That(trail.To(3), Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(trail.From(3), Is.EqualTo(new GridPosition(3, 3)));
        }

        [Test]
        public void AFrameThatSwallowedSeveralTicksSnapsInsteadOfStretching()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(false);
            snake.Step(false);
            trail.Sync();

            Assert.That(trail.To(0), Is.EqualTo(new GridPosition(7, 3)));
            AssertStandingStill(trail);
        }

        [Test]
        public void TheHeadFacesWhereTheSnakeIsGoing()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            Assert.That(trail.Facing(0), Is.EqualTo(Direction.Right));

            snake.TryTurn(Direction.Up);
            snake.Step(false);
            trail.Sync();

            Assert.That(trail.Facing(0), Is.EqualTo(Direction.Up));
        }

        [Test]
        public void EveryOtherPartFacesTheOneAheadOfIt()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.TryTurn(Direction.Up);
            snake.Step(false);
            trail.Sync();

            // The body now reads (5,4) (5,3) (4,3): a corner one step after the turn.
            Assert.That(trail.Facing(1), Is.EqualTo(Direction.Up));
            Assert.That(trail.Facing(2), Is.EqualTo(Direction.Right));
        }

        [Test]
        public void RestartingForgetsTheStepThatCameBefore()
        {
            Snake snake = NewSnake();
            SnakeTrail trail = Watching(snake);

            snake.Step(false);
            trail.Sync();

            snake.Reset(new GridPosition(5, 3), Direction.Right, 3);
            trail.Restart();
            trail.Sync();

            AssertStandingStill(trail);
        }

        [Test]
        public void ASnakeIsRequired()
        {
            Assert.Throws<ArgumentNullException>(() => new SnakeTrail(null));
        }
    }
}
