using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelItemsTests
    {
        private static LevelItemData Wall(int x, int y)
        {
            return new LevelItemData { Type = "Wall", X = x, Y = y };
        }

        // A 4x3 board with the snake lying along the middle row, so no test
        // spoils the placement by accident. Each test spoils exactly one item.
        private static LevelData Furnished(params LevelItemData[] items)
        {
            return new LevelData
            {
                Width = 4,
                Height = 3,
                StartX = 2,
                StartY = 1,
                StartDirection = "Right",
                SnakeLength = 2,
                FoodGoal = 0,
                Items = items
            };
        }

        private static Level Create(LevelData data)
        {
            Level level;
            Result result = Level.TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        private static Result Reject(LevelData data)
        {
            Level level;
            Result result = Level.TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False, "The item list should not have been accepted.");
            Assert.That(level, Is.Null);

            return result;
        }

        [Test]
        public void EveryItemLandsOnTheCellItNames()
        {
            Level level = Create(Furnished(Wall(0, 0), Wall(3, 2)));

            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(level.GroundAt(new GridPosition(3, 2)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(level.GroundAt(new GridPosition(1, 1)), Is.EqualTo(CellGroundType.Open));
        }

        [Test]
        public void TheCoordinatesAreTheOnesTheRestOfTheGameUses()
        {
            Level level = Create(Furnished(Wall(0, 2)));

            Assert.That(level.GroundAt(new GridPosition(0, 2)), Is.EqualTo(CellGroundType.Wall),
                "Y counts up from the bottom, the same way GridPosition does everywhere else.");
            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Open));
        }

        [Test]
        public void ALevelWithoutItemsGetsAnOpenBoard()
        {
            LevelData data = Furnished();
            data.Items = null;

            Level level = Create(data);

            for (int cell = 0; cell < level.Board.CellCount; cell++)
            {
                Assert.That(level.GroundAt(level.Board.FromIndex(cell)), Is.EqualTo(CellGroundType.Open));
            }
        }

        [Test]
        public void AnItemOffTheBoardIsRejected()
        {
            Result result = Reject(Furnished(Wall(0, 0), Wall(4, 0)));

            Assert.That(result.Error, Does.Contain("Item 2").And.Contain("(4, 0)"),
                "The message should name which item and where it went.");
        }

        [Test]
        public void TwoItemsOnOneCellAreRejected()
        {
            Result result = Reject(Furnished(Wall(0, 0), Wall(0, 0)));

            Assert.That(result.Error, Does.Contain("Item 2").And.Contain("(0, 0)").And.Contain("Wall"));
        }

        [Test]
        public void AnItemWithoutATypeIsRejected()
        {
            LevelData missing = Furnished(new LevelItemData { Type = null, X = 0, Y = 0 });
            LevelData blank = Furnished(new LevelItemData { Type = "   ", X = 0, Y = 0 });

            Assert.That(Reject(missing).Error, Does.Contain("does not say what it is"));
            Assert.That(Reject(blank).IsSuccess, Is.False);
        }

        [Test]
        public void AnUnknownTypeNameIsRejected()
        {
            Result result = Reject(Furnished(new LevelItemData { Type = "Brick", X = 0, Y = 0 }));

            Assert.That(result.Error, Does.Contain("Brick"), "The message should name what was wrong.");
        }

        [Test]
        public void ATypeWrittenAsANumberIsRejected()
        {
            LevelData data = Furnished(new LevelItemData { Type = "9", X = 0, Y = 0 });

            Assert.That(Reject(data).IsSuccess, Is.False,
                "Enum.TryParse accepts stray numbers, so a defined name is required.");
        }

        [Test]
        public void AnItemThatPlacesNothingIsRejected()
        {
            Result result = Reject(Furnished(new LevelItemData { Type = "Open", X = 0, Y = 0 }));

            Assert.That(result.Error, Does.Contain("Open"),
                "An item that puts nothing down is a file the editor should never have written.");
        }

        [Test]
        public void TypeNamesAreReadWithoutRegardToCase()
        {
            Level level = Create(Furnished(new LevelItemData { Type = "wAlL", X = 0, Y = 0 }));

            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Wall));
        }

        [Test]
        public void AMissingItemIsRejected()
        {
            Assert.That(Reject(Furnished(Wall(0, 0), null)).Error, Does.Contain("Item 2"));
        }

        [Test]
        public void GroundOutsideTheBoardCannotBeAskedFor()
        {
            Level level = Create(Furnished(Wall(0, 0)));

            Assert.Throws<ArgumentOutOfRangeException>(() => level.GroundAt(new GridPosition(4, 0)));
        }

        [Test]
        public void ATunnelIsPlacedLikeAnyOtherItem()
        {
            Level level = Create(Furnished(new LevelItemData { Type = "Tunnel", X = 0, Y = 0 }));

            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Tunnel),
                "A new ground type costs a name in the file and nothing in the loader.");
        }
    }
}
