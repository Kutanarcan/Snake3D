using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class SnakeCellsTests
    {
        private static readonly GridSize Board = new GridSize(6, 6);

        private static SnakeCells NewSnakeCells(GridMap map, Snake snake)
        {
            return new SnakeCells(map, snake);
        }

        [Test]
        public void AMapAndASnakeAreBothRequired()
        {
            GridMap map = new GridMap(Board);
            Snake snake = new Snake(Board.CellCount);

            Assert.Throws<ArgumentNullException>(() => new SnakeCells(null, snake));
            Assert.Throws<ArgumentNullException>(() => new SnakeCells(map, null));
        }

        [Test]
        public void OccupyClaimsEveryBodyCell()
        {
            GridMap map = new GridMap(Board);
            Snake snake = new Snake(Board.CellCount);
            snake.Reset(new GridPosition(2, 2), Direction.Right, 3);

            NewSnakeCells(map, snake).Occupy();

            Assert.That(map[new GridPosition(2, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(map[new GridPosition(1, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(map[new GridPosition(0, 2)], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount - 3));
        }

        [Test]
        public void ReleaseGivesEveryBodyCellBack()
        {
            GridMap map = new GridMap(Board);
            Snake snake = new Snake(Board.CellCount);
            snake.Reset(new GridPosition(2, 2), Direction.Right, 3);
            SnakeCells snakeCells = NewSnakeCells(map, snake);
            snakeCells.Occupy();

            snakeCells.Release();

            Assert.That(map[new GridPosition(2, 2)], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount));
        }

        [Test]
        public void AClearStretchCanHoldTheBody()
        {
            GridMap map = new GridMap(Board);
            SnakeCells snakeCells = NewSnakeCells(map, new Snake(Board.CellCount));

            Result room = snakeCells.CanOccupy(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(room.IsSuccess, Is.True);
            Assert.That(room.Error, Is.Null);
        }

        [Test]
        public void ThePlacementIsMeasuredBackwardsFromTheHead()
        {
            GridMap map = new GridMap(Board);
            map.SetGround(new GridPosition(2, 1), CellGroundType.Wall);
            SnakeCells snakeCells = NewSnakeCells(map, new Snake(Board.CellCount));

            // Facing up, the body trails downwards, straight into the wall below.
            Result room = snakeCells.CanOccupy(new GridPosition(2, 2), Direction.Up, 3);

            Assert.That(room.IsSuccess, Is.False);
            Assert.That(room.Error, Does.Contain("(2, 1)"));
        }

        [Test]
        public void ABodyThatWouldLeaveTheGridIsRefused()
        {
            GridMap map = new GridMap(Board);
            SnakeCells snakeCells = NewSnakeCells(map, new Snake(Board.CellCount));

            Result room = snakeCells.CanOccupy(new GridPosition(0, 2), Direction.Right, 3);

            Assert.That(room.IsSuccess, Is.False);
            Assert.That(room.Error, Does.Contain("(-1, 2)"));
        }

        [Test]
        public void ABodyThatWouldCrossAnOccupiedCellIsRefused()
        {
            GridMap map = new GridMap(Board);
            map.SetGround(new GridPosition(1, 2), CellGroundType.Wall);
            SnakeCells snakeCells = NewSnakeCells(map, new Snake(Board.CellCount));

            Result room = snakeCells.CanOccupy(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(room.IsSuccess, Is.False);
            Assert.That(room.Error, Does.Contain("Wall").And.Contain("(1, 2)"));
        }

        [Test]
        public void AskingWhetherThereIsRoomChangesNothing()
        {
            GridMap map = new GridMap(Board);
            SnakeCells snakeCells = NewSnakeCells(map, new Snake(Board.CellCount));

            snakeCells.CanOccupy(new GridPosition(2, 2), Direction.Right, 3);

            Assert.That(map.FreeCount, Is.EqualTo(Board.CellCount), "The query wrote to the map.");
        }
    }
}
