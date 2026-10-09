using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class GameSessionTests
    {
        private static readonly GridSize Board = new GridSize(6, 6);
        private const float Interval = 0.1f;

        private ScriptedInputSource _input;

        // Every run in here heads Right on a 6x6 board, so a test only has to
        // say where the head starts, how long the body is, where the food is
        // pinned and what the goal asks for.
        private static Level NewLevel(GridPosition head, int length, int foodGoal)
        {
            LevelData data = new LevelData
            {
                Width = Board.Width,
                Height = Board.Height,
                StartX = head.X,
                StartY = head.Y,
                StartDirection = "Right",
                SnakeLength = length,
                FoodGoal = foodGoal
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        // (0,0) is the corner none of these runs walk through, so the food stays
        // out of the way unless a test pins it somewhere on purpose.
        private GameSession NewSession(GridPosition head, int length, out SnakeGame game)
        {
            PinnedRandomSource random;
            return NewSession(head, length, new GridPosition(0, 0), 0, out game, out random);
        }

        private GameSession NewSession(GridPosition head, int length, GridPosition foodCell, int foodGoal,
            out SnakeGame game, out PinnedRandomSource random)
        {
            Level level = NewLevel(head, length, foodGoal);
            GridMap map = new GridMap(level.Board);

            random = new PinnedRandomSource(map, foodCell);
            game = new SnakeGame(map, new Snake(level.Board.CellCount),
                new AppleSpawnManager(new FoodSpawner(map, random), new GoldenAppleSpawner(map, random, 0, 0)));
            _input = new ScriptedInputSource();

            return new GameSession(game, new TickTimer(Interval), _input, level);
        }

        // A run waits for the player before it moves, so a test that wants steps
        // has to ask for the first one. Right is the way every level in here
        // already faces, so this sets the run off and steers nothing, and the
        // empty frame spends no time doing it.
        private void Start(GameSession session)
        {
            session.StartRun();

            _input.Turn(Direction.Right);
            session.Tick(0f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Running), "The run never set off.");
        }

        [Test]
        public void TheGameTheTimerTheInputAndTheLevelAreAllRequired()
        {
            SnakeGame game;
            NewSession(new GridPosition(2, 2), 3, out game);
            Level level = NewLevel(new GridPosition(2, 2), 3, 0);

            Assert.Throws<ArgumentNullException>(() => new GameSession(null, new TickTimer(Interval), new ScriptedInputSource(), level));
            Assert.Throws<ArgumentNullException>(() => new GameSession(game, null, new ScriptedInputSource(), level));
            Assert.Throws<ArgumentNullException>(() => new GameSession(game, new TickTimer(Interval), null, level));
            Assert.Throws<ArgumentNullException>(() => new GameSession(game, new TickTimer(Interval), new ScriptedInputSource(), null));
        }

        [Test]
        public void AFreshSessionIsWaitingToStart()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready));
            Assert.That(session.FoodEaten, Is.EqualTo(0));
            Assert.That(session.IsOver, Is.False);
        }

        [Test]
        public void TimeIsIgnoredUntilTheRunStarts()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);

            session.Tick(Interval * 10f);

            Assert.That(game.Snake.Length, Is.EqualTo(0));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready));
        }

        [Test]
        public void StartingTheRunLaysTheLevelOutAndWaits()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);

            session.StartRun();

            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(game.Snake.Length, Is.EqualTo(3));
            Assert.That(game.Food.IsOnTheBoard, Is.True);
        }

        [Test]
        public void TheFirstDirectionSetsTheRunOff()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            session.StartRun();

            _input.Turn(Direction.Up);
            session.Tick(Interval);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Running));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)),
                "The snake goes the way it was asked to, not the way the level was facing.");
        }

        [Test]
        public void AWaitingRunIgnoresADirectionItCannotTake()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            session.StartRun();

            // The level faces Right and the body is laid out behind the head, so
            // Left is a turn the snake could never make.
            _input.Turn(Direction.Left);
            session.Tick(Interval * 3f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void TheTimeSpentWaitingIsNotBanked()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            session.StartRun();

            session.Tick(Interval * 5f);

            _input.Turn(Direction.Right);
            session.Tick(Interval * 0.9f);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)),
                "A run that banked the waiting would jump a step the moment it set off.");
        }

        [Test]
        public void TheSessionHandsTheGameToWhoeverDrawsIt()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);

            session.StartRun();

            Assert.That(session.Game, Is.SameAs(game));
        }

        [Test]
        public void TimeShorterThanTheIntervalMovesNothing()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            Start(session);

            session.Tick(Interval * 0.9f);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void EveryWholeIntervalIsOneStep()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            Start(session);

            session.Tick(Interval);
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 2)));

            session.Tick(Interval);
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(4, 2)));
        }

        [Test]
        public void ALongFrameIsDrainedIntoSeveralSteps()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(1, 2), 2, out game);
            Start(session);

            session.Tick(Interval * 3f);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(4, 2)),
                "Three whole intervals in one frame are three steps.");
        }

        [Test]
        public void EatingCountsAndAnnouncesItself()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 0, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);
            int announcements = 0;
            session.Ate += () => announcements++;

            session.Tick(Interval);

            Assert.That(session.FoodEaten, Is.EqualTo(1));
            Assert.That(announcements, Is.EqualTo(1));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Running));
        }

        [Test]
        public void ReachingTheGoalWinsTheRunAndStopsTheSteps()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 1, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);
            int wins = 0;
            session.Won += () => wins++;

            // Two intervals in one frame: the run must win on the first and
            // never take the second.
            session.Tick(Interval * 2f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(wins, Is.EqualTo(1));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 2)));
        }

        [Test]
        public void EatingPutsTheNextPieceOfFoodOnTheBoard()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 0, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);

            session.Tick(Interval);

            Assert.That(game.Food.IsOnTheBoard, Is.True);
            Assert.That(game.Food.Position, Is.EqualTo(new GridPosition(5, 5)));
            Assert.That(game.Map[new GridPosition(5, 5)], Is.EqualTo(CellOccupantType.Food));
        }

        [Test]
        public void TheWinningBiteLeavesTheBoardEmpty()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 1, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);

            session.Tick(Interval);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(game.Food.IsOnTheBoard, Is.False);
            AssertNoFoodIsOnTheBoard(game);
        }

        [Test]
        public void TheBoardIsAlreadyRightWhenTheBiteIsAnnounced()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 1, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);

            // Whoever draws the food redraws it on Ate, so the last bite must not
            // announce itself while an apple is still standing.
            bool foodWasStillThere = true;
            session.Ate += () => foodWasStillThere = game.Food.IsOnTheBoard;

            session.Tick(Interval);

            Assert.That(foodWasStillThere, Is.False);
        }

        [Test]
        public void AGoldenAppleScoresThreeAgainstTheGoal()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 6, out game, out _);
            Start(session);
            game.Map.Set(new GridPosition(3, 2), CellOccupantType.GoldenApple);

            session.Tick(Interval);

            Assert.That(session.FoodEaten, Is.EqualTo(3));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Running), "Six were asked for and three were eaten.");
        }

        [Test]
        public void AGoldenAppleLeavesThePlainAppleWhereItIs()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 6, out game, out _);
            Start(session);
            game.Map.Set(new GridPosition(3, 2), CellOccupantType.GoldenApple);

            session.Tick(Interval);

            Assert.That(game.Food.Position, Is.EqualTo(new GridPosition(0, 0)), "The bonus must not move the apple.");
            Assert.That(game.Map[new GridPosition(0, 0)], Is.EqualTo(CellOccupantType.Food));
        }

        [Test]
        public void AGoldenAppleCanCarryTheRunPastTheGoal()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 2, out game, out _);
            Start(session);
            game.Map.Set(new GridPosition(3, 2), CellOccupantType.GoldenApple);

            session.Tick(Interval);

            Assert.That(session.FoodEaten, Is.EqualTo(3));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Won), "Two were asked for and three arrived at once.");
        }

        private static void AssertNoFoodIsOnTheBoard(SnakeGame game)
        {
            for (int y = 0; y < Board.Height; y++)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    GridPosition cell = new GridPosition(x, y);
                    Assert.That(game.Map[cell], Is.Not.EqualTo(CellOccupantType.Food),
                        "An apple was left on " + cell + " after the run was over.");
                }
            }
        }

        [Test]
        public void TheGoalComesFromTheLevel()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 7, out game, out random);

            Assert.That(session.FoodGoal, Is.EqualTo(7));
        }

        [Test]
        public void AnEndlessRunKeepsGoingPastEveryBite()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 0, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);

            session.Tick(Interval);

            Assert.That(session.FoodEaten, Is.EqualTo(1));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Running));
        }

        [Test]
        public void DyingLosesTheRunAndStopsTheSteps()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(5, 2), 3, out game);
            Start(session);
            int losses = 0;
            session.Lost += () => losses++;

            session.Tick(Interval * 3f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(session.IsOver, Is.True);
            Assert.That(losses, Is.EqualTo(1));
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(5, 2)), "A fatal step changes nothing.");
        }

        [Test]
        public void AFinishedRunIgnoresFurtherTime()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(5, 2), 3, out game);
            Start(session);
            session.Tick(Interval);
            int freeAfterDeath = game.Map.FreeCount;

            session.Tick(Interval * 4f);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(5, 2)));
            Assert.That(game.Map.FreeCount, Is.EqualTo(freeAfterDeath));
        }

        [Test]
        public void StartingAgainClearsTheProgressAndTheClock()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(3, 2), 0, out game, out random);
            Start(session);
            random.Target = new GridPosition(5, 5);
            session.Tick(Interval);
            session.Tick(Interval * 0.9f);

            random.Target = new GridPosition(0, 0);
            Start(session);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Running));
            Assert.That(session.FoodEaten, Is.EqualTo(0));

            session.Tick(Interval * 0.9f);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)),
                "The leftover time from the previous run must not carry over.");
        }

        [Test]
        public void ClearedListenersAreNoLongerCalled()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(5, 2), 3, out game);
            Start(session);
            int losses = 0;
            session.Lost += () => losses++;

            session.ClearListeners();
            session.Tick(Interval);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(losses, Is.EqualTo(0));
        }

        [Test]
        public void APressedDirectionSteersTheNextStep()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 0, out game, out random);
            Start(session);

            _input.Turn(Direction.Up);
            session.Tick(Interval);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void TheInputIsSampledOncePerFrameHoweverManyStepsFollow()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(1, 2), 2, new GridPosition(0, 0), 0, out game, out random);
            Start(session);
            int readsAtTheStart = _input.ReadCount;

            _input.Turn(Direction.Up);
            session.Tick(Interval * 3f);

            Assert.That(_input.ReadCount, Is.EqualTo(readsAtTheStart + 1),
                "The player pressed once, so the frame reads once.");
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(1, 5)), "All three steps follow the same turn.");
        }

        [Test]
        public void AReversalAskedForByTheInputIsRefused()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 0, out game, out random);
            Start(session);

            _input.Turn(Direction.Left);
            session.Tick(Interval);

            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(3, 2)), "Turning back into the body is refused.");
        }

        [Test]
        public void StartingARunDropsAPressThatWasStillWaiting()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(2, 2), 3, new GridPosition(0, 0), 0, out game, out random);
            session.StartRun();

            _input.Turn(Direction.Up);
            session.StartRun();
            session.Tick(Interval);

            Assert.That(_input.ClearCount, Is.EqualTo(2));
            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready),
                "A key pressed before the restart must not set the new run off.");
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void AFinishedRunNoLongerReadsTheInput()
        {
            SnakeGame game;
            PinnedRandomSource random;
            GameSession session = NewSession(new GridPosition(5, 2), 3, new GridPosition(0, 0), 0, out game, out random);
            Start(session);
            session.Tick(Interval);
            int readsAtDeath = _input.ReadCount;

            _input.Turn(Direction.Up);
            session.Tick(Interval * 4f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(_input.ReadCount, Is.EqualTo(readsAtDeath));
        }

        [Test]
        public void SteeringSomewhereNewIsReported()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            Start(session);

            int turns = 0;
            session.Turned += () => turns++;

            _input.Turn(Direction.Up);
            session.Tick(0f);

            Assert.That(turns, Is.EqualTo(1));
        }

        [Test]
        public void PressingTheWayTheSnakeAlreadyGoesSteersNothing()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            session.StartRun();

            int turns = 0;
            session.Turned += () => turns++;

            _input.Turn(Direction.Right);
            session.Tick(0f);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Running), "The run still has to set off.");
            Assert.That(turns, Is.EqualTo(0));
        }

        [Test]
        public void ATurnTheSnakeRefusesIsNotReported()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            Start(session);

            int turns = 0;
            session.Turned += () => turns++;

            _input.Turn(Direction.Left);
            session.Tick(0f);

            Assert.That(turns, Is.EqualTo(0));
        }

        [Test]
        public void BothPressesInsideOneTickAreHonoured()
        {
            SnakeGame game;
            GameSession session = NewSession(new GridPosition(2, 2), 3, out game);
            Start(session);

            _input.Turn(Direction.Up);
            session.Tick(0f);
            _input.Turn(Direction.Left);
            session.Tick(0f);

            session.Tick(Interval);
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(2, 3)));

            session.Tick(Interval);
            Assert.That(game.Snake.Head, Is.EqualTo(new GridPosition(1, 3)),
                "The first press was overwritten instead of queued.");
        }
    }
}
