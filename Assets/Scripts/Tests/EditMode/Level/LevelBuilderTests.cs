using System;
using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelBuilderTests
    {
        private const float Interval = 0.1f;

        private ScriptedInputSource _input;

        private static Level NewLevel(int width, int height, int length, int foodGoal)
        {
            LevelData data = new LevelData
            {
                Width = width,
                Height = height,
                StartX = 3,
                StartY = 3,
                StartDirection = "Right",
                SnakeLength = length,
                FoodGoal = foodGoal
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        // An 8x5 board walled all the way round, leaving a 6x3 room the snake
        // starts in facing the right-hand wall.
        private static Level NewWalledLevel()
        {
            LevelData data = new LevelData
            {
                Width = 8,
                Height = 5,
                StartX = 4,
                StartY = 2,
                StartDirection = "Right",
                SnakeLength = 3,
                FoodGoal = 0,
                Items = Ring(8, 5)
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        private static LevelItemData[] Ring(int width, int height)
        {
            List<LevelItemData> walls = new List<LevelItemData>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        walls.Add(new LevelItemData { Type = "Wall", X = x, Y = y });
                    }
                }
            }

            return walls.ToArray();
        }

        private GameSession Build(Level level)
        {
            _input = new ScriptedInputSource();

            return LevelBuilder.Build(level, Interval, new ScriptedRandomSource(), _input);
        }

        // A run waits for the player before it moves, so a test that wants steps
        // has to ask for the first one. Every level in here already faces Right,
        // so this sets the run off and steers nothing.
        private void Start(GameSession session)
        {
            session.StartRun();

            _input.Turn(Direction.Right);
            session.Tick(0f);
        }

        [Test]
        public void TheBoardIsBuiltAtTheLevelsSize()
        {
            GameSession session = Build(NewLevel(8, 5, 3, 0));

            Assert.That(session.Game.Map.Size.Width, Is.EqualTo(8));
            Assert.That(session.Game.Map.Size.Height, Is.EqualTo(5));
        }

        [Test]
        public void TheItemsLandOnTheMapAsGround()
        {
            GridMap map = Build(NewWalledLevel()).Game.Map;

            Assert.That(map.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(map.GroundAt(new GridPosition(7, 4)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(map.GroundAt(new GridPosition(4, 2)), Is.EqualTo(CellGroundType.Open));
        }

        [Test]
        public void WalledCellsAreNeverOfferedToTheFoodSpawner()
        {
            GridMap map = Build(NewWalledLevel()).Game.Map;

            Assert.That(map.FreeCount, Is.EqualTo(6 * 3),
                "Only the room inside the ring is free, and the ground goes down before anything stands on it.");
        }

        [Test]
        public void TheWallsFromTheLevelAreFatal()
        {
            GameSession session = Build(NewWalledLevel());
            Start(session);

            // (5,2), then (6,2), then (7,2) -- which is the right-hand wall.
            session.Tick(Interval);
            session.Tick(Interval);
            Assert.That(session.Status, Is.EqualTo(GameStatus.Running));

            session.Tick(Interval);

            Assert.That(session.Status, Is.EqualTo(GameStatus.Lost));
        }

        [Test]
        public void TheSnakeCanGrowIntoEveryCell()
        {
            GameSession session = Build(NewLevel(8, 5, 3, 0));

            Assert.That(session.Game.Snake.Capacity, Is.GreaterThanOrEqualTo(8 * 5),
                "A snake that fills the board must still fit in its body buffer.");
        }

        [Test]
        public void TheBuiltSessionIsWaitingRatherThanRunning()
        {
            GameSession session = Build(NewLevel(8, 8, 3, 0));

            Assert.That(session.Status, Is.EqualTo(GameStatus.Ready));
            Assert.That(session.Game.Snake.Length, Is.EqualTo(0), "Nothing is placed until the run starts.");
        }

        [Test]
        public void StartingTheRunLaysTheSnakeOutTheWayTheLevelDescribesIt()
        {
            GameSession session = Build(NewLevel(8, 8, 4, 0));

            session.StartRun();

            Assert.That(session.Game.Snake.Head, Is.EqualTo(new GridPosition(3, 3)));
            Assert.That(session.Game.Snake.Length, Is.EqualTo(4));
            Assert.That(session.Game.Food.IsOnTheBoard, Is.True);
        }

        [Test]
        public void TheStartDirectionIsTheOneTheLevelAsksFor()
        {
            GameSession session = Build(NewLevel(8, 8, 3, 0));
            Start(session);

            session.Tick(Interval);

            Assert.That(session.Game.Snake.Head, Is.EqualTo(new GridPosition(4, 3)), "Right means +X.");
        }

        [Test]
        public void TheTickIntervalDrivesTheSteps()
        {
            GameSession session = Build(NewLevel(8, 8, 3, 0));
            Start(session);

            session.Tick(Interval * 0.9f);
            Assert.That(session.Game.Snake.Head, Is.EqualTo(new GridPosition(3, 3)));

            session.Tick(Interval * 0.2f);
            Assert.That(session.Game.Snake.Head, Is.EqualTo(new GridPosition(4, 3)),
                "The leftover 0.9 plus 0.2 crosses one whole interval, so exactly one step follows.");
        }

        [Test]
        public void TheFoodGoalCarriesIntoTheSession()
        {
            GameSession session = Build(NewLevel(8, 8, 3, 6));

            Assert.That(session.FoodGoal, Is.EqualTo(6));
        }

        [Test]
        public void TheLevelTheRandomSourceAndTheInputAreAllRequired()
        {
            Level level = NewLevel(8, 8, 3, 0);

            Assert.Throws<ArgumentNullException>(() => LevelBuilder.Build(null, Interval, new ScriptedRandomSource(), new ScriptedInputSource()));
            Assert.Throws<ArgumentNullException>(() => LevelBuilder.Build(level, Interval, null, new ScriptedInputSource()));
            Assert.Throws<ArgumentNullException>(() => LevelBuilder.Build(level, Interval, new ScriptedRandomSource(), null));
        }

        [Test]
        public void ATickIntervalThatIsNotPositiveIsRefusedByTheTimer()
        {
            Level level = NewLevel(8, 8, 3, 0);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => LevelBuilder.Build(level, 0f, new ScriptedRandomSource(), new ScriptedInputSource()),
                "Speed is not a level rule, so TickTimer stays the only place that guards it.");
        }
    }
}
