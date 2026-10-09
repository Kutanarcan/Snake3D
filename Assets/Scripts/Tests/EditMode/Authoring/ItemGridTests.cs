using System;
using Game.Authoring;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class ItemGridTests
    {
        private static readonly GridSize Board = new GridSize(6, 4);

        private static LevelItemData NewItem(string type, int x, int y)
        {
            return new LevelItemData { Type = type, X = x, Y = y };
        }

        [Test]
        public void ANewGridIsEmpty()
        {
            ItemGrid grid = new ItemGrid(Board);

            Assert.That(grid.ItemAt(new GridPosition(2, 2)), Is.Null);
            Assert.That(grid.ToItems(), Is.Empty);
        }

        [Test]
        public void PaintingPutsAnItemOnTheCellItWasGiven()
        {
            ItemGrid grid = new ItemGrid(Board);

            grid.Paint(new GridPosition(2, 3), CellGroundType.Tunnel);

            LevelItemData item = grid.ItemAt(new GridPosition(2, 3));

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Type, Is.EqualTo("Tunnel"), "The name is what a level file will carry.");
            Assert.That(item.X, Is.EqualTo(2));
            Assert.That(item.Y, Is.EqualTo(3));
        }

        [Test]
        public void PaintingOverACellReplacesWhatWasThere()
        {
            ItemGrid grid = new ItemGrid(Board);

            grid.Paint(new GridPosition(1, 1), CellGroundType.Wall);
            grid.Paint(new GridPosition(1, 1), CellGroundType.Tunnel);

            Assert.That(grid.ItemAt(new GridPosition(1, 1)).Type, Is.EqualTo("Tunnel"));
            Assert.That(grid.ToItems().Length, Is.EqualTo(1), "One cell holds one item.");
        }

        [Test]
        public void ErasingLeavesTheCellEmpty()
        {
            ItemGrid grid = new ItemGrid(Board);
            grid.Paint(new GridPosition(1, 1), CellGroundType.Wall);

            grid.Erase(new GridPosition(1, 1));

            Assert.That(grid.ItemAt(new GridPosition(1, 1)), Is.Null);
            Assert.That(grid.ToItems(), Is.Empty);
        }

        [Test]
        public void OpenIsNotAThingToPaint()
        {
            ItemGrid grid = new ItemGrid(Board);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.Paint(new GridPosition(1, 1), CellGroundType.Open));
        }

        [Test]
        public void ACellOutsideTheBoardIsARejectedCall()
        {
            ItemGrid grid = new ItemGrid(Board);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.Paint(new GridPosition(6, 0), CellGroundType.Wall));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.ItemAt(new GridPosition(-1, 0)));
        }

        [Test]
        public void AFilesItemsComeBackOnTheCellsTheyName()
        {
            LevelItemData[] items =
            {
                NewItem("Wall", 0, 0),
                NewItem("Tunnel", 5, 3)
            };

            ItemGrid grid = ItemGrid.From(items, Board);

            Assert.That(grid.ItemAt(new GridPosition(0, 0)).Type, Is.EqualTo("Wall"));
            Assert.That(grid.ItemAt(new GridPosition(5, 3)).Type, Is.EqualTo("Tunnel"));
            Assert.That(grid.ItemAt(new GridPosition(1, 1)), Is.Null);
        }

        [Test]
        public void TheSameItemsComeBackOutAgain()
        {
            LevelItemData[] items =
            {
                NewItem("Wall", 0, 0),
                NewItem("Wall", 4, 1),
                NewItem("Tunnel", 5, 3)
            };

            LevelItemData[] written = ItemGrid.From(items, Board).ToItems();

            Assert.That(written.Length, Is.EqualTo(3));
            Assert.That(written, Is.EquivalentTo(items), "The grid must not rewrite what it was handed.");
        }

        [Test]
        public void NoItemsAtAllIsAnEmptyGridRatherThanACrash()
        {
            Assert.That(ItemGrid.From(null, Board).ToItems(), Is.Empty);
        }

        [Test]
        public void AFileThatStacksTwoItemsOnOneCellIsAWiringBug()
        {
            LevelItemData[] items =
            {
                NewItem("Wall", 2, 2),
                NewItem("Tunnel", 2, 2)
            };

            Assert.Throws<ArgumentException>(() => ItemGrid.From(items, Board));
        }

        [Test]
        public void ShrinkingTheBoardKeepsWhatStillFitsAndDropsTheRest()
        {
            ItemGrid grid = new ItemGrid(Board);
            grid.Paint(new GridPosition(1, 1), CellGroundType.Wall);
            grid.Paint(new GridPosition(5, 3), CellGroundType.Tunnel);

            grid.Resize(new GridSize(3, 3));

            Assert.That(grid.Board.Width, Is.EqualTo(3));
            Assert.That(grid.ItemAt(new GridPosition(1, 1)).Type, Is.EqualTo("Wall"));
            Assert.That(grid.ToItems().Length, Is.EqualTo(1), "The corner item no longer has a cell.");
        }

        [Test]
        public void GrowingTheBoardLeavesEveryItemWhereItWas()
        {
            ItemGrid grid = new ItemGrid(Board);
            grid.Paint(new GridPosition(5, 3), CellGroundType.Wall);

            grid.Resize(new GridSize(9, 9));

            Assert.That(grid.ItemAt(new GridPosition(5, 3)).Type, Is.EqualTo("Wall"),
                "A wider board must not slide items along the row.");
            Assert.That(grid.ToItems().Length, Is.EqualTo(1));
        }
    }
}
