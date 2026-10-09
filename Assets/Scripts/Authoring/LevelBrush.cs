using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Authoring
{
    public readonly struct LevelBrush
    {
        public static readonly LevelBrush Erase = new LevelBrush(CellGroundType.Open, false);
        public static readonly LevelBrush Start = new LevelBrush(CellGroundType.Open, true);

        private LevelBrush(CellGroundType type, bool movesTheStart)
        {
            Type = type;
            MovesTheStart = movesTheStart;
        }

        public CellGroundType Type { get; }
        public bool MovesTheStart { get; }

        public bool Erases
        {
            get { return !MovesTheStart && Type == CellGroundType.Open; }
        }

        public string Name
        {
            get
            {
                if (MovesTheStart)
                {
                    return "Start";
                }

                return Erases ? "Erase" : Type.ToString();
            }
        }

        public static LevelBrush Item(CellGroundType type)
        {
            if (type == CellGroundType.Open)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Open is the erase brush, not an item.");
            }

            return new LevelBrush(type, false);
        }

        public static IEnumerable<LevelBrush> All()
        {
            yield return Erase;
            yield return Start;

            foreach (CellGroundType type in Enum.GetValues(typeof(CellGroundType)))
            {
                if (type != CellGroundType.Open)
                {
                    yield return Item(type);
                }
            }
        }

        public void Apply(LevelDraft draft, GridPosition cell)
        {
            if (draft == null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            if (MovesTheStart)
            {
                draft.Start = cell;
                return;
            }

            if (Erases)
            {
                draft.Items.Erase(cell);
                return;
            }

            draft.Items.Paint(cell, Type);
        }
    }
}
