using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class GridSizeTests
    {
        [Test]
        public void Constructor_RejectsNonPositiveDimensions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridSize(0, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridSize(5, -1));
        }

        [Test]
        public void CellCount_IsWidthTimesHeight()
        {
            Assert.That(new GridSize(9, 13).CellCount, Is.EqualTo(117));
        }

        [Test]
        public void Contains_AcceptsCornersAndRejectsOutside()
        {
            GridSize size = new GridSize(4, 3);

            Assert.That(size.Contains(new GridPosition(0, 0)), Is.True);
            Assert.That(size.Contains(new GridPosition(3, 2)), Is.True);

            Assert.That(size.Contains(new GridPosition(-1, 0)), Is.False);
            Assert.That(size.Contains(new GridPosition(0, -1)), Is.False);
            Assert.That(size.Contains(new GridPosition(4, 0)), Is.False);
            Assert.That(size.Contains(new GridPosition(0, 3)), Is.False);
        }

        [Test]
        public void ToIndexAndFromIndex_RoundTripEveryCellOfARectangle()
        {
            GridSize size = new GridSize(4, 3);

            for (int index = 0; index < size.CellCount; index++)
            {
                GridPosition position = size.FromIndex(index);

                Assert.That(size.Contains(position), Is.True, "FromIndex produced an out of bounds cell at " + index);
                Assert.That(size.ToIndex(position), Is.EqualTo(index));
            }
        }
    }
}
