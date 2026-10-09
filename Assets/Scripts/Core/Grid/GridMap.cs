using System;

namespace Game.Core
{
    // Two layers over one grid: the ground a cell is made of, and the piece
    // standing on it. They are kept apart because a piece has to be able to
    // leave without taking the ground with it -- a snake crossing a cell must
    // put back what was underneath, not Empty.
    //
    // The free list is the conjunction of the two: a cell can take a new piece
    // only when its ground is Open and nothing is standing there.
    public sealed class GridMap
    {
        private readonly CellGroundType[] _ground;
        private readonly CellOccupantType[] _occupants;
        private FreeCellSet _freeCells;

        public GridMap(GridSize size)
        {
            Size = size;
            _ground = new CellGroundType[size.CellCount];
            _occupants = new CellOccupantType[size.CellCount];
            _freeCells = new FreeCellSet(size.CellCount);
        }

        public GridSize Size { get; }

        public int FreeCount
        {
            get { return _freeCells.Count; }
        }

        public CellOccupantType this[GridPosition position]
        {
            get { return _occupants[CellIndexOf(position)]; }
        }

        public CellGroundType GroundAt(GridPosition position)
        {
            return _ground[CellIndexOf(position)];
        }

        public bool IsFree(GridPosition position)
        {
            return IsFreeAt(CellIndexOf(position));
        }

        public GridPosition FreeCellAt(int slot)
        {
            return Size.FromIndex(_freeCells[slot]);
        }

        public void Set(GridPosition position, CellOccupantType occupant)
        {
            int cell = CellIndexOf(position);
            if (_occupants[cell] == occupant)
            {
                return;
            }

            bool wasFree = IsFreeAt(cell);
            _occupants[cell] = occupant;

            Refresh(cell, wasFree);
        }

        public void SetGround(GridPosition position, CellGroundType ground)
        {
            int cell = CellIndexOf(position);
            if (_ground[cell] == ground)
            {
                return;
            }

            bool wasFree = IsFreeAt(cell);
            _ground[cell] = ground;

            Refresh(cell, wasFree);
        }

        public void Clear()
        {
            Array.Clear(_ground, 0, _ground.Length);
            Array.Clear(_occupants, 0, _occupants.Length);
            _freeCells.Fill();
        }

        private bool IsFreeAt(int cell)
        {
            return _ground[cell] == CellGroundType.Open && _occupants[cell] == CellOccupantType.Empty;
        }

        // A write only reaches the free list when it crosses the boundary
        // between free and taken; the layer that moved does not matter.
        private void Refresh(int cell, bool wasFree)
        {
            bool isFree = IsFreeAt(cell);
            if (isFree == wasFree)
            {
                return;
            }

            if (isFree)
            {
                _freeCells.Add(cell);
            }
            else
            {
                _freeCells.Remove(cell);
            }
        }

        private int CellIndexOf(GridPosition position)
        {
            if (!Size.Contains(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position + " is outside the grid.");
            }

            return Size.ToIndex(position);
        }
    }
}
