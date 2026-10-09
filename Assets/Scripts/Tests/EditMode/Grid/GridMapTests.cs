using System;
using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class GridMapTests
    {
        private static HashSet<GridPosition> FreeCells(GridMap map)
        {
            HashSet<GridPosition> cells = new HashSet<GridPosition>();
            for (int slot = 0; slot < map.FreeCount; slot++)
            {
                Assert.That(cells.Add(map.FreeCellAt(slot)), Is.True, "The free list reported the same cell twice.");
            }

            return cells;
        }

        private static void AssertFreeListMatchesTheFreeCells(GridMap map)
        {
            HashSet<GridPosition> free = FreeCells(map);

            for (int index = 0; index < map.Size.CellCount; index++)
            {
                GridPosition position = map.Size.FromIndex(index);

                Assert.That(free.Contains(position), Is.EqualTo(map.IsFree(position)),
                    position + " is " + map.GroundAt(position) + " carrying " + map[position]
                    + " but the free list says otherwise.");
            }
        }

        [Test]
        public void ANewMapIsEmptyAndEveryCellIsFree()
        {
            GridMap map = new GridMap(new GridSize(4, 3));

            Assert.That(map.FreeCount, Is.EqualTo(12));
            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void OccupyingACellRemovesItFromTheFreeList()
        {
            GridMap map = new GridMap(new GridSize(4, 3));

            map.SetGround(new GridPosition(2, 1), CellGroundType.Wall);

            Assert.That(map.GroundAt(new GridPosition(2, 1)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(map.FreeCount, Is.EqualTo(11));
            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void ClearingACellReturnsItToTheFreeList()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            GridPosition position = new GridPosition(0, 2);

            map.Set(position, CellOccupantType.Food);
            map.Set(position, CellOccupantType.Empty);

            Assert.That(map[position], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(map.FreeCount, Is.EqualTo(12));
            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void ReplacingOneContentWithAnotherKeepsTheCellOccupied()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            GridPosition position = new GridPosition(1, 1);

            map.Set(position, CellOccupantType.Food);
            map.Set(position, CellOccupantType.Snake);

            Assert.That(map[position], Is.EqualTo(CellOccupantType.Snake));
            Assert.That(map.FreeCount, Is.EqualTo(11), "The cell was already occupied, so nothing left the free list.");
            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void WritingTheSameContentTwiceChangesNothing()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            GridPosition position = new GridPosition(3, 0);

            map.SetGround(position, CellGroundType.Wall);
            map.SetGround(position, CellGroundType.Wall);
            Assert.That(map.FreeCount, Is.EqualTo(11));

            map.SetGround(position, CellGroundType.Open);
            map.SetGround(position, CellGroundType.Open);
            Assert.That(map.FreeCount, Is.EqualTo(12));

            map.Set(position, CellOccupantType.Snake);
            map.Set(position, CellOccupantType.Snake);
            Assert.That(map.FreeCount, Is.EqualTo(11));

            map.Set(position, CellOccupantType.Empty);
            map.Set(position, CellOccupantType.Empty);
            Assert.That(map.FreeCount, Is.EqualTo(12));

            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void ACellIsFreeOnlyWhenBothLayersAreEmpty()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            GridPosition position = new GridPosition(1, 1);

            Assert.That(map.IsFree(position), Is.True, "A fresh cell is open and unoccupied.");

            map.SetGround(position, CellGroundType.Wall);
            Assert.That(map.IsFree(position), Is.False, "CellGroundType alone can take a cell out of the free list.");

            map.Set(position, CellOccupantType.Snake);
            Assert.That(map.FreeCount, Is.EqualTo(11), "The cell was already taken; the second layer must not remove it twice.");

            map.SetGround(position, CellGroundType.Open);
            Assert.That(map.IsFree(position), Is.False, "The occupant still stands there.");

            map.Set(position, CellOccupantType.Empty);
            Assert.That(map.IsFree(position), Is.True, "Both layers are clear again.");
            Assert.That(map.FreeCount, Is.EqualTo(12));

            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void GroundSurvivesAnOccupantLeaving()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            GridPosition position = new GridPosition(2, 0);

            map.SetGround(position, CellGroundType.Wall);
            map.Set(position, CellOccupantType.Snake);
            map.Set(position, CellOccupantType.Empty);

            Assert.That(map.GroundAt(position), Is.EqualTo(CellGroundType.Wall), "The piece took the ground with it.");
            Assert.That(map[position], Is.EqualTo(CellOccupantType.Empty));
            Assert.That(map.FreeCount, Is.EqualTo(11), "A walled cell is not free just because nothing stands on it.");

            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void TheFreeListStaysConsistentThroughManyWritesAndErases()
        {
            GridSize size = new GridSize(5, 4);
            GridMap map = new GridMap(size);

            // Occupy every third cell, then release every other occupied one, so the
            // swap-remove hits both the middle and the end of the free list.
            for (int index = 0; index < size.CellCount; index += 3)
            {
                map.Set(size.FromIndex(index), CellOccupantType.Snake);
                AssertFreeListMatchesTheFreeCells(map);
            }

            for (int index = 0; index < size.CellCount; index += 6)
            {
                map.Set(size.FromIndex(index), CellOccupantType.Empty);
                AssertFreeListMatchesTheFreeCells(map);
            }
        }

        [Test]
        public void AFullMapReportsNoFreeCells()
        {
            GridSize size = new GridSize(3, 3);
            GridMap map = new GridMap(size);

            for (int index = 0; index < size.CellCount; index++)
            {
                map.SetGround(size.FromIndex(index), CellGroundType.Wall);
            }

            Assert.That(map.FreeCount, Is.EqualTo(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.FreeCellAt(0));
        }

        [Test]
        public void ClearResetsEveryCell()
        {
            GridSize size = new GridSize(4, 3);
            GridMap map = new GridMap(size);
            map.SetGround(new GridPosition(0, 0), CellGroundType.Wall);
            map.Set(new GridPosition(2, 2), CellOccupantType.Food);

            map.Clear();

            Assert.That(map.FreeCount, Is.EqualTo(size.CellCount));
            AssertFreeListMatchesTheFreeCells(map);
        }

        [Test]
        public void CellsOutsideTheGridAreRejected()
        {
            GridMap map = new GridMap(new GridSize(4, 3));

            Assert.Throws<ArgumentOutOfRangeException>(() => map.SetGround(new GridPosition(4, 0), CellGroundType.Wall));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.Set(new GridPosition(0, -1), CellOccupantType.Snake));
            Assert.Throws<ArgumentOutOfRangeException>(() => { CellOccupantType unused = map[new GridPosition(-1, 0)]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { CellGroundType unused = map.GroundAt(new GridPosition(0, 3)); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { bool unused = map.IsFree(new GridPosition(-1, -1)); });
        }

        [Test]
        public void FreeCellAtRejectsSlotsOutsideTheFreeRange()
        {
            GridMap map = new GridMap(new GridSize(4, 3));
            map.SetGround(new GridPosition(0, 0), CellGroundType.Wall);

            Assert.Throws<ArgumentOutOfRangeException>(() => map.FreeCellAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.FreeCellAt(map.FreeCount));
        }
    }
}
