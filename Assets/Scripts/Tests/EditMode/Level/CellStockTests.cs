using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class CellStockTests
    {
        private static Level NewLevel(int side, int snakeLength, int foodGoal)
        {
            LevelData data = new LevelData
            {
                Width = side,
                Height = side,
                StartX = side / 2,
                StartY = side / 2,
                StartDirection = "Right",
                SnakeLength = snakeLength,
                FoodGoal = foodGoal
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        // The same level with a run of walls along the bottom row, well clear of
        // the body the level lays out across the middle.
        private static Level NewWalledLevel(int side, int walls)
        {
            LevelItemData[] items = new LevelItemData[walls];

            for (int x = 0; x < walls; x++)
            {
                items[x] = new LevelItemData { Type = "Wall", X = x, Y = 0 };
            }

            LevelData data = new LevelData
            {
                Width = side,
                Height = side,
                StartX = side / 2,
                StartY = side / 2,
                StartDirection = "Right",
                SnakeLength = 3,
                FoodGoal = 2,
                Items = items
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        private static LevelSequence NewSequence(params Level[] levels)
        {
            return new LevelSequence(levels);
        }

        [Test]
        public void TheGroundStockCoversTheLevelThatLaysTheMost()
        {
            CellStock stock = CellStock.For(NewSequence(
                NewWalledLevel(8, 6),
                NewWalledLevel(9, 2)));

            Assert.That(stock.Ground, Is.EqualTo(6));
        }

        [Test]
        public void ALevelWithoutItemsNeedsNoGroundStock()
        {
            CellStock stock = CellStock.For(NewSequence(NewLevel(16, 3, 10)));

            Assert.That(stock.Ground, Is.EqualTo(0));
        }

        [Test]
        public void ASequenceIsRequired()
        {
            Assert.Throws<ArgumentNullException>(() => CellStock.For(null));
        }

        [Test]
        public void OneLevelNeedsItsBoardAndTheBodyItCanGrowTo()
        {
            CellStock stock = CellStock.For(NewSequence(NewLevel(16, 3, 10)));

            Assert.That(stock.Environment, Is.EqualTo(16 * 16));
            Assert.That(stock.Organic, Is.EqualTo(3 + 10 + 2), "Three parts, ten bites, and the two apples that can share the board.");
        }

        [Test]
        public void AnEndlessLevelCanFillItsBoard()
        {
            CellStock stock = CellStock.For(NewSequence(NewLevel(8, 3, 0)));

            Assert.That(stock.Organic, Is.EqualTo(8 * 8));
        }

        [Test]
        public void TheStockIsCountedForTheHungriestLevelInTheSequence()
        {
            CellStock stock = CellStock.For(NewSequence(
                NewLevel(20, 3, 4),
                NewLevel(10, 5, 30),
                NewLevel(12, 3, 6)));

            Assert.That(stock.Environment, Is.EqualTo(20 * 20), "The widest board decides the ground.");
            Assert.That(stock.Organic, Is.EqualTo(5 + 30 + 2), "The longest snake decides the pieces.");
        }

        [Test]
        public void TheTwoCountsAreTakenFromWhicheverLevelNeedsThemMost()
        {
            // The wider board is not the hungriest one: the 8x8 level carries the
            // long goal, the 9x9 level only sets the board size. Neither level is
            // the answer to both questions on its own.
            CellStock stock = CellStock.For(NewSequence(
                NewLevel(8, 3, 40),
                NewLevel(9, 3, 2)));

            Assert.That(stock.Environment, Is.EqualTo(9 * 9));
            Assert.That(stock.Organic, Is.EqualTo(3 + 40 + 2));
        }
    }
}
