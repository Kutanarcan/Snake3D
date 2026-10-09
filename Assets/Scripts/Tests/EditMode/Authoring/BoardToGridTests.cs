using System.Collections.Generic;
using Game.Authoring;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class BoardToGridTests
    {
        // Four wide and two tall, so a transpose cannot hide behind a square.
        private static readonly GridSize Board = new GridSize(4, 2);

        [Test]
        public void TheGridIsTheBoardTurnedAQuarter()
        {
            BoardToGrid grid = new BoardToGrid(Board);

            Assert.That(grid.Rows, Is.EqualTo(4), "Rows run along board X.");
            Assert.That(grid.Columns, Is.EqualTo(2), "Columns run along board Y.");
        }

        [Test]
        public void TheOriginIsDrawnInTheBottomRightCorner()
        {
            BoardToGrid grid = new BoardToGrid(Board);
            GridPosition origin = new GridPosition(0, 0);

            Assert.That(grid.RowOf(origin), Is.EqualTo(grid.Rows - 1), "The last row is the bottom one.");
            Assert.That(grid.ColumnOf(origin), Is.EqualTo(grid.Columns - 1), "The last column is the rightmost.");
        }

        // The camera looks down a 135 degree yaw: board X climbs the screen and
        // board Y runs to the left. These are the four corners it puts where.
        [Test]
        public void EachCornerLandsWhereTheCameraShowsIt()
        {
            BoardToGrid grid = new BoardToGrid(Board);

            Assert.That(grid.Cell(0, 0), Is.EqualTo(new GridPosition(3, 1)), "Top left is the far corner.");
            Assert.That(grid.Cell(0, 1), Is.EqualTo(new GridPosition(3, 0)), "Top right is the greatest X.");
            Assert.That(grid.Cell(3, 0), Is.EqualTo(new GridPosition(0, 1)), "Bottom left is the greatest Y.");
            Assert.That(grid.Cell(3, 1), Is.EqualTo(new GridPosition(0, 0)), "Bottom right is the origin.");
        }

        // The four a designer reported: they picked Right and watched the snake
        // point up. Direction names the board, the board is drawn a quarter turn
        // round, and these are the pairs that come out of it.
        [TestCase(Direction.Right, Direction.Up)]
        [TestCase(Direction.Up, Direction.Left)]
        [TestCase(Direction.Down, Direction.Right)]
        [TestCase(Direction.Left, Direction.Down)]
        public void AHeadingIsDrawnAQuarterTurnFromItsName(Direction heading, Direction drawnAs)
        {
            Assert.That(BoardToGrid.DrawnAs(heading), Is.EqualTo(drawnAs));
            Assert.That(BoardToGrid.Heading(drawnAs), Is.EqualTo(heading), "The arrow has to write back what it drew.");
        }

        [Test]
        public void TheQuarterTurnNeverLandsOnTheSameHeadingTwice()
        {
            List<Direction> drawn = new List<Direction>();

            foreach (Direction heading in System.Enum.GetValues(typeof(Direction)))
            {
                drawn.Add(BoardToGrid.DrawnAs(heading));
            }

            Assert.That(drawn, Is.Unique, "Four headings must fill four arrows.");
        }

        [Test]
        public void EveryCellSurvivesTheTripThereAndBack()
        {
            BoardToGrid grid = new BoardToGrid(Board);

            for (int row = 0; row < grid.Rows; row++)
            {
                for (int column = 0; column < grid.Columns; column++)
                {
                    GridPosition cell = grid.Cell(row, column);

                    Assert.That(Board.Contains(cell), Is.True, "Row " + row + " column " + column + " left the board.");
                    Assert.That(grid.RowOf(cell), Is.EqualTo(row));
                    Assert.That(grid.ColumnOf(cell), Is.EqualTo(column));
                }
            }
        }
    }
}
