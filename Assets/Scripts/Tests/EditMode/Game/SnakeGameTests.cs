using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SnakeGameTests
    {
        private static readonly GridSize Board = new GridSize(6, 6);

        // (0,0) is the corner none of these tests walk through, so the food stays
        // out of the way unless a test pins it somewhere on purpose.
        private static SnakeGame NewGame()
        {
            return NewGame(new GridPosition(0, 0), out _);
        }

        private static SnakeGame NewGame(GridPosition foodCell, out PinnedRandomSource random)
        {
            GridMap map = new GridMap(Board);
            random = new PinnedRandomSource(map, foodCell);
            return new SnakeGame(map, new Snake(Board.CellCount), NewApples(map, random));
        }

        // The golden apple is switched off unless a test asks for it: its clock
        // would otherwise drop a bonus into the middle of runs about something
        // else. Tests that want one put it on the map themselves.
        private static AppleSpawnManager NewApples(GridMap map, IRandomSource random)
        {
            return new AppleSpawnManager(new FoodSpawner(map, random), new GoldenAppleSpawner(map, random, 0, 0));
        }

        private static void SealTheBorder(GridMap map)
        {
            for (int x = 0; x < map.Size.Width; x++)
            {
                map.SetGround(new GridPosition(x, 0), CellGroundType.Wall);
                map.SetGround(new GridPosition(x, map.Size.Height - 1), CellGroundType.Wall);
            }

            for (int y = 0; y < map.Size.Height; y++)
            {
                map.SetGround(new GridPosition(0, y), CellGroundType.Wall);
                map.SetGround(new GridPosition(map.Size.Width - 1, y), CellGroundType.Wall);
            }
        }

        private static void AssertTheMapMatchesTheSnake(SnakeGame game, int occupiedByTheLevel)
        {
            for (int i = 0; i < game.Snake.Length; i++)
            {
                Assert.That(game.Map[game.Snake[i]], Is.EqualTo(CellOccupantType.Snake),
                    "Body part " + i + " at " + game.Snake[i] + " is missing from the map.");
            }

            int food = game.Food.IsOnTheBoard ? 1 : 0;

            Assert.That(game.Map.FreeCount,
                Is.EqualTo(Board.CellCount - occupiedByTheLevel - game.Snake.Length - food),
                "A cell leaked into or out of the free list.");
        }

        [Test]
        public void ASnakeThatCannotGrowIntoEveryCellIsRejected()
        {
            GridMap map = new GridMap(Board);
            AppleSpawnManager apples = NewApples(map, new ScriptedRandomSource());

            Assert.Throws<ArgumentException>(() => new SnakeGame(map, new Snake(4), apples));
        }

        [Test]
        public void ResetPutsFoodOnTheBoard()
        {
            SnakeGame game = NewGame(new GridPosition(5, 5), out _);

            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(game.Food.IsOnTheBoard, Is.True);
            Assert.That(game.Food.Position, Is.EqualTo(new GridPosition(5, 5)));
            Assert.That(game.Map[new GridPosition(5, 5)], Is.EqualTo(CellOccupantType.Food));
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void ResetWritesEveryBodyCellIntoTheMap()
        {
            SnakeGame game = NewGame();

            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(game.Map[new GridPosition(2, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(game.Map[new GridPosition(1, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(game.Map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Snake));
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void AStartOnAnOccupiedCellIsRejected()
        {
            SnakeGame game = NewGame();
            game.Map.SetGround(new GridPosition(1, 2), CellGroundType.Wall);

            ArgumentException rejection = Assert.Throws<ArgumentException>(
                () => game.Reset(new GridPosition(2, 2), Direction.Right, 3));

            Assert.That(rejection.Message, Does.Contain("Wall").And.Contain("(1, 2)"), "The reason did not survive the throw.");
            Assert.That(game.Snake.Length, Is.EqualTo(0), "A rejected placement must leave the snake untouched.");
        }

        [Test]
        public void AStartThatHangsOutsideTheGridIsRejected()
        {
            SnakeGame game = NewGame();

            ArgumentException rejection = Assert.Throws<ArgumentException>(
                () => game.Reset(new GridPosition(0, 2), Direction.Right, 3));

            Assert.That(rejection.Message, Does.Contain("(-1, 2)"), "The tail would hang off the left edge.");
        }

        [Test]
        public void ResettingAgainClearsThePreviousBodyFromTheMap()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            game.Reset(new GridPosition(4, 5), Direction.Right, 3);

            Assert.That(game.Map[new GridPosition(2, 2)], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(game.Map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Empty));
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void AStepIntoAnEmptyCellMovesTheHeadAndFreesTheTail()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(game.Snake.Length, Is.EqualTo(3));
            Assert.That(game.Map[new GridPosition(3, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(game.Map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Empty), "The tail cell was not released.");
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void ATurnTakesEffectOnTheNextStep()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(game.TryTurn(Direction.Up), Is.True);
            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void LeavingTheGridIsFatal()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(5, 2), Direction.Right, 3);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Died));
        }

        [Test]
        public void HittingAWallIsFatal()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Wall);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Died));
        }

        [Test]
        public void TheBorderRingIsJustWallsAndKillsLikeOne()
        {
            SnakeGame game = NewGame(new GridPosition(2, 4), out _);
            SealTheBorder(game.Map);
            game.Reset(new GridPosition(2, 2), Direction.Right, 2);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));
            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));
            Assert.That(game.Step(), Is.EqualTo(StepResult.Died), "(5,2) sits on the border ring.");
        }

        [Test]
        public void RunningIntoItsOwnBodyIsFatal()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(4, 2), Direction.Right, 5);

            game.TryTurn(Direction.Up);
            game.Step();
            game.TryTurn(Direction.Left);
            game.Step();
            game.TryTurn(Direction.Down);

            // The head is at (3,3) and (3,2) is a body part, while the tail sits at (2,2).
            Assert.That(game.Step(), Is.EqualTo(StepResult.Died));
        }

        [Test]
        public void TheTailCellBeingVacatedThisTickCanBeEntered()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(3, 2), Direction.Right, 4);

            game.TryTurn(Direction.Up);
            game.Step();
            game.TryTurn(Direction.Left);
            game.Step();
            game.TryTurn(Direction.Down);

            // The head is at (2,3) and (2,2) is the tail, which this very step releases.
            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void AFoodWorthSeveralCellsIsPaidOffOneCellPerStep()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            game.Feed(3);
            Assert.That(game.Snake.Length, Is.EqualTo(3), "Feeding owes the cells, it does not hand them over.");

            for (int step = 1; step <= 3; step++)
            {
                game.Step();

                Assert.That(game.Snake.Length, Is.EqualTo(3 + step), "Step " + step + " paid the wrong amount.");
                Assert.That(game.Snake.Tail, Is.EqualTo(new GridPosition(0, 2)), "The tail waits while the debt runs.");
            }

            game.TryTurn(Direction.Up);
            game.Step();

            Assert.That(game.Snake.Length, Is.EqualTo(6), "The debt is paid, so the body stops lengthening.");
            Assert.That(game.Snake.Tail, Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(game.Map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Empty),
                "The tail cell is released again once nothing is owed.");
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void AGoldenAppleIsWorthThreeCellsAndArrivesOneStepAtATime()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.Set(new GridPosition(3, 2), CellOccupantType.GoldenApple);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Ate));
            Assert.That(game.LastMeal, Is.EqualTo(CellOccupantType.GoldenApple));
            Assert.That(game.Snake.Length, Is.EqualTo(4), "One of the three cells is paid on the spot.");

            game.Step();
            Assert.That(game.Snake.Length, Is.EqualTo(5));

            game.Step();
            Assert.That(game.Snake.Length, Is.EqualTo(6));

            game.TryTurn(Direction.Up);
            game.Step();

            Assert.That(game.Snake.Length, Is.EqualTo(6), "Three cells were owed and three were paid.");
        }

        [Test]
        public void APlainStepReportsNoMealAtAll()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            game.Step();

            Assert.That(game.LastMeal, Is.EqualTo(CellOccupantType.Empty));
        }

        [Test]
        public void FollowingTheTailIsFatalWhileGrowthIsOwed()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(3, 2), Direction.Right, 4);

            game.TryTurn(Direction.Up);
            game.Step();
            game.TryTurn(Direction.Left);
            game.Step();
            game.TryTurn(Direction.Down);

            // The same corner the snake may cut when it owes nothing: (2,2) is the
            // tail, but a cell of growth keeps it there for the head to hit.
            game.Feed(1);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Died));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)), "A fatal step moves nothing.");
        }

        [Test]
        public void AFoodMustBeWorthAtLeastOneCell()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => game.Feed(0));
        }

        [Test]
        public void GrowthOwedByTheLastRunIsNotCarriedIntoTheNextOne()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Feed(3);

            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Step();

            Assert.That(game.Snake.Length, Is.EqualTo(3));
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void EatingGrowsTheSnakeAndKeepsTheTail()
        {
            SnakeGame game = NewGame(new GridPosition(3, 2), out PinnedRandomSource random);
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            random.Target = new GridPosition(5, 5);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Ate));

            Assert.That(game.Snake.Length, Is.EqualTo(4));
            Assert.That(game.Snake.Tail, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(game.Map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Snake),
                "The tail was released although the snake grew.");
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void EatingTakesTheFoodOffTheBoardAndLeavesTheNextOneToTheRun()
        {
            SnakeGame game = NewGame(new GridPosition(3, 2), out PinnedRandomSource random);
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            random.Target = new GridPosition(5, 5);

            game.Step();

            Assert.That(game.Food.IsOnTheBoard, Is.False);
            Assert.That(game.Map[new GridPosition(5, 5)], Is.EqualTo(CellOccupantType.Empty),
                "A step must not decide that another apple follows.");
            Assert.That(game.Map[new GridPosition(3, 2)], Is.EqualTo(CellOccupantType.Snake), "The eaten cell now carries the head.");
            AssertTheMapMatchesTheSnake(game, 0);
        }

        [Test]
        public void AFatalStepChangesNothing()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Wall);
            int freeBefore = game.Map.FreeCount;

            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(game.Snake.Length, Is.EqualTo(3));
            Assert.That(game.Map.FreeCount, Is.EqualTo(freeBefore));
            AssertTheMapMatchesTheSnake(game, 1);
        }

        [Test]
        public void TheMapStaysConsistentOverALapAroundTheBoard()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Direction[] lap = { Direction.Right, Direction.Up, Direction.Left, Direction.Down };
            foreach (Direction direction in lap)
            {
                game.TryTurn(direction);
                for (int step = 0; step < 2; step++)
                {
                    Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));
                    AssertTheMapMatchesTheSnake(game, 0);
                }
            }
        }

        [Test]
        public void ATunnelTurnsTheHeadAsItArrives()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved), "A tunnel is not fatal.");
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 2)),
                "The head spends this step on the tunnel cell.");
            Assert.That(game.Snake.Direction, Is.EqualTo(Direction.Up), "Right goes in, Up comes out.");

            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 3)), "It leaves the far side next step.");
        }

        [Test]
        public void ATunnelSurvivesTheSnakePassingOverIt()
        {
            // Heading Down into the tunnel at (4,2) turns the snake Left, which
            // leaves it four cells of room -- enough for the tail to clear.
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(4, 3), Direction.Down, 3);

            GridPosition tunnel = new GridPosition(4, 2);
            game.Map.SetGround(tunnel, CellGroundType.Tunnel);

            for (int step = 0; step < 4; step++)
            {
                Assert.That(game.Step(), Is.EqualTo(StepResult.Moved), "Step " + step + " ended the run.");
            }

            Assert.That(game.Map.GroundAt(tunnel), Is.EqualTo(CellGroundType.Tunnel),
                "The body left, so the ground under it has to still be there.");
            Assert.That(game.Map[tunnel], Is.EqualTo(CellOccupantType.Empty));
        }

        [Test]
        public void ATunnelBeatsWhateverThePlayerAsksForOnTheSameStep()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);

            Assert.That(game.TryTurn(Direction.Up), Is.True);
            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)),
                "The turn was honoured, so the head never reached the tunnel.");
            Assert.That(game.Snake.Direction, Is.EqualTo(Direction.Up));
        }

        [Test]
        public void SteeringBesideATunnelDoesNotLetTheSnakeRunThroughIt()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);

            game.Step();

            // The head is on the tunnel and the player is still pressing the way
            // it came in. Several frames pass here before the next step.
            Assert.That(game.TryTurn(Direction.Right), Is.False);

            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 3)),
                "Pressing Right on the tunnel would have carried the head to (4, 2).");
        }

        [Test]
        public void TheSnakeIsSteerableAgainOnceItHasLeftTheTunnel()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);

            game.Step();
            game.Step();

            Assert.That(game.TryTurn(Direction.Right), Is.True);

            game.Step();

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(4, 3)));
        }

        [Test]
        public void AStepThatSurvivesNamesNoDeath()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            game.Step();

            Assert.That(game.LastDeath, Is.EqualTo(DeathCause.None));
        }

        // The cell that stopped the head is off the board here, which is the
        // point: the run must read the cause before it reads the cell.
        [Test]
        public void LeavingTheGridNamesTheBoardEdge()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(5, 2), Direction.Right, 3);

            game.Step();

            Assert.That(game.LastDeath, Is.EqualTo(DeathCause.Bounds));
            Assert.That(game.LastDeathCell, Is.EqualTo(new GridPosition(6, 2)));
        }

        // What kind of ground killed is left for the map to answer, so a new
        // deadly ground type costs this class nothing.
        [Test]
        public void HittingAWallNamesTheGroundAndWhereItStands()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Wall);

            game.Step();

            Assert.That(game.LastDeath, Is.EqualTo(DeathCause.Ground));
            Assert.That(game.LastDeathCell, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(game.Map.GroundAt(game.LastDeathCell), Is.EqualTo(CellGroundType.Wall));
        }

        [Test]
        public void RunningIntoItsOwnBodyNamesTheSnake()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(4, 2), Direction.Right, 5);

            game.TryTurn(Direction.Up);
            game.Step();
            game.TryTurn(Direction.Left);
            game.Step();
            game.TryTurn(Direction.Down);
            game.Step();

            Assert.That(game.LastDeath, Is.EqualTo(DeathCause.Snake));
            Assert.That(game.LastDeathCell, Is.EqualTo(new GridPosition(3, 2)));
        }

        [Test]
        public void ResetForgetsHowTheLastRunEnded()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(5, 2), Direction.Right, 3);
            game.Step();

            game.Reset(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(game.LastDeath, Is.EqualTo(DeathCause.None));
        }

        [Test]
        public void ATunnelReportsTheStepItTurned()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);

            Assert.That(game.Step(), Is.EqualTo(StepResult.Moved));
            Assert.That(game.LastStepDeflected, Is.True);
        }

        [Test]
        public void AStepOverOpenGroundIsNoDeflection()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Tunnel);
            game.Step();

            game.Step();

            Assert.That(game.LastStepDeflected, Is.False, "The tunnel is behind the head now.");
        }
    }
}
