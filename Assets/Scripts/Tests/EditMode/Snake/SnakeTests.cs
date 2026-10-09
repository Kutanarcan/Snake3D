using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SnakeTests
    {
        private static Snake CreateSnake(GridPosition head, Direction direction, int length)
        {
            Snake snake = new Snake(64);
            snake.Reset(head, direction, length);
            return snake;
        }

        [Test]
        public void Step_MovesOneCellInTheCurrentDirection()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);

            GridPosition head = snake.Step(false);

            Assert.That(head, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(snake.Head, Is.EqualTo(head));
            Assert.That(snake.Length, Is.EqualTo(3));
        }

        [Test]
        public void Step_WithGrow_AddsOnePartAndKeepsTheTail()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);
            GridPosition tailBefore = snake.Tail;

            snake.Step(true);

            Assert.That(snake.Length, Is.EqualTo(4));
            Assert.That(snake.Tail, Is.EqualTo(tailBefore));
        }

        [Test]
        public void TryTurn_TakesEffectOnTheNextStep()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Up), Is.True);
            Assert.That(snake.Direction, Is.EqualTo(Direction.Right), "The turn must not apply before the tick.");

            snake.Step(false);

            Assert.That(snake.Direction, Is.EqualTo(Direction.Up));
            Assert.That(snake.Head, Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void TryTurn_RejectsAReversal()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Left), Is.False);

            snake.Step(false);

            Assert.That(snake.Direction, Is.EqualTo(Direction.Right));
            Assert.That(snake.Head, Is.EqualTo(new GridPosition(3, 2)));
        }

        [Test]
        public void TryTurn_TwiceWithinOneTick_WalksBothTurnsInOrder()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Up, 3);

            Assert.That(snake.TryTurn(Direction.Left), Is.True);
            Assert.That(snake.TryTurn(Direction.Down), Is.True,
                "Down is checked against Left, the turn already waiting -- Up, Left, Down never reverses.");

            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(snake.Direction, Is.EqualTo(Direction.Left));

            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(snake.Direction, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void TryTurn_ReversalIsAllowedOnceTheSnakeHasActuallyTurned()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Up, 3);

            snake.TryTurn(Direction.Left);
            snake.Step(false);

            Assert.That(snake.TryTurn(Direction.Down), Is.True);
            Assert.That(snake.TryTurn(Direction.Up), Is.False,
                "Up is the reverse of the turn already waiting.");
        }

        [Test]
        public void EveryPartFollowsThePathTheHeadTook()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);

            snake.TryTurn(Direction.Up);
            snake.Step(false);
            snake.Step(false);

            Assert.That(snake[0], Is.EqualTo(new GridPosition(2, 4)));
            Assert.That(snake[1], Is.EqualTo(new GridPosition(2, 3)), "The body part should stand where the head was one tick ago.");
            Assert.That(snake[2], Is.EqualTo(new GridPosition(2, 2)), "The tail should stand where the head was two ticks ago.");
        }

        [Test]
        public void PeekNextHead_ReportsTheBufferedTurnWithoutMoving()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);
            snake.TryTurn(Direction.Up);

            Assert.That(snake.PeekNextHead(), Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(snake.Head, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void IndexOf_FindsTheOwnPartTheHeadIsAboutToEnter()
        {
            Snake snake = CreateSnake(new GridPosition(4, 2), Direction.Right, 5);

            snake.TryTurn(Direction.Up);
            snake.Step(false);
            snake.TryTurn(Direction.Left);
            snake.Step(false);
            snake.TryTurn(Direction.Down);

            // Body is (3,3) (4,3) (4,2) (3,2) (2,2) and the head is about to enter (3,2).
            Assert.That(snake.PeekNextHead(), Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(snake.IndexOf(snake.PeekNextHead()), Is.EqualTo(3),
                "The rules layer tests the target cell before stepping, because after the step the head is always index 0.");
        }

        [Test]
        public void IndexOf_ReportsTheTailCellTheSnakeIsAboutToVacate()
        {
            Snake snake = CreateSnake(new GridPosition(3, 0), Direction.Right, 4);

            snake.TryTurn(Direction.Up);
            snake.Step(false);
            snake.TryTurn(Direction.Left);
            snake.Step(false);
            snake.TryTurn(Direction.Down);

            // Entering your own tail is legal while not growing, so the rules layer
            // needs to tell the tail index apart from the rest of the body.
            Assert.That(snake.IndexOf(snake.PeekNextHead()), Is.EqualTo(snake.Length - 1));
        }

        [Test]
        public void Reset_ClearsAPreviouslyBufferedTurn()
        {
            Snake snake = CreateSnake(new GridPosition(2, 2), Direction.Right, 3);
            snake.TryTurn(Direction.Up);

            snake.Reset(new GridPosition(5, 5), Direction.Left, 2);
            snake.Step(false);

            Assert.That(snake.Direction, Is.EqualTo(Direction.Left));
            Assert.That(snake.Head, Is.EqualTo(new GridPosition(4, 5)));
        }

        [Test]
        public void AForcedTurnIsNotRefusedAsAUTurn()
        {
            Snake snake = new Snake(16);
            snake.Reset(new GridPosition(4, 4), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Left), Is.False, "The player may not double back.");

            snake.Force(Direction.Left);

            Assert.That(snake.Direction, Is.EqualTo(Direction.Left),
                "The board is not asking, so the guard does not apply to it.");
        }

        [Test]
        public void AForcedTurnThrowsAwayWhateverThePlayerHadQueued()
        {
            Snake snake = new Snake(16);
            snake.Reset(new GridPosition(4, 4), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Up), Is.True);
            snake.Force(Direction.Down);

            Assert.That(snake.PeekNextHead(), Is.EqualTo(new GridPosition(4, 3)),
                "The queued turn would have taken the head to (4, 5).");
        }

        [Test]
        public void ThePlayerCannotSteerOutOfAForcedTurnBeforeTheSnakeHasMoved()
        {
            Snake snake = new Snake(16);
            snake.Reset(new GridPosition(4, 4), Direction.Right, 3);

            snake.Force(Direction.Up);

            Assert.That(snake.TryTurn(Direction.Right), Is.False,
                "The board has just answered this; a press in the meantime does not reopen it.");
            Assert.That(snake.Direction, Is.EqualTo(Direction.Up));
        }

        [Test]
        public void SteeringComesBackTheStepAfterAForcedTurn()
        {
            Snake snake = new Snake(16);
            snake.Reset(new GridPosition(4, 4), Direction.Right, 3);

            snake.Force(Direction.Up);
            snake.Step(false);

            Assert.That(snake.TryTurn(Direction.Right), Is.True,
                "The hold covers one step, not the rest of the run.");
        }

        [Test]
        public void TwoTurnsAskedForBeforeAStepBothHappen()
        {
            Snake snake = CreateSnake(new GridPosition(4, 4), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Up), Is.True);
            Assert.That(snake.TryTurn(Direction.Left), Is.True);

            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(4, 5)));
            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(3, 5)));
        }

        [Test]
        public void ATurnIsJudgedAgainstTheOneAlreadyWaiting()
        {
            Snake snake = CreateSnake(new GridPosition(4, 4), Direction.Right, 3);
            snake.TryTurn(Direction.Up);

            Assert.That(snake.TryTurn(Direction.Down), Is.False,
                "Down is not the reverse of Right, but it is the reverse of the turn already waiting.");

            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(4, 5)));
        }

        [Test]
        public void PressingTheWayTheSnakeIsAlreadyGoingQueuesNothing()
        {
            Snake snake = CreateSnake(new GridPosition(4, 4), Direction.Right, 3);

            Assert.That(snake.TryTurn(Direction.Right), Is.True, "The press still counts, it just steers nothing.");
            Assert.That(snake.TryTurn(Direction.Up), Is.True);
            Assert.That(snake.TryTurn(Direction.Left), Is.True, "Room was never taken by the repeat.");
        }

        [Test]
        public void APressWithNowhereToWaitIsTurnedDown()
        {
            Snake snake = CreateSnake(new GridPosition(4, 4), Direction.Right, 3);

            snake.TryTurn(Direction.Up);
            snake.TryTurn(Direction.Left);

            Assert.That(snake.TryTurn(Direction.Down), Is.False, "The queue is " + InputQueue.Depth + " deep.");
        }

        [Test]
        public void ATunnelDropsWhateverWasWaiting()
        {
            Snake snake = CreateSnake(new GridPosition(4, 4), Direction.Right, 3);
            snake.TryTurn(Direction.Up);

            snake.Force(Direction.Down);

            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(snake.Step(false), Is.EqualTo(new GridPosition(4, 2)),
                "The turn the player had queued died with the deflection.");
        }
    }
}
