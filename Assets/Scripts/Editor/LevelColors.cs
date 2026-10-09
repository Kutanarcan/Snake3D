using Game.Core;
using UnityEngine;

namespace Game.Editor
{
    // Every colour the level tool draws with. One place, so an item type added
    // to CellGroundType needs one row here and shows up in both the palette and
    // the grid. A type nobody has coloured comes out magenta on purpose.
    internal static class LevelColors
    {
        // An empty cell has to read as a cell. The first try sat at 0.22, which
        // is the editor's own background, so an untouched board looked like
        // nothing had been drawn at all.
        internal static readonly Color Empty = new Color(0.34f, 0.34f, 0.36f);
        internal static readonly Color Lines = new Color(0.13f, 0.13f, 0.14f);
        internal static readonly Color Head = new Color(0.36f, 0.85f, 0.42f);
        internal static readonly Color Body = new Color(0.22f, 0.45f, 0.26f);
        internal static readonly Color Selected = new Color(0.95f, 0.80f, 0.25f);

        internal static Color Of(CellGroundType type)
        {
            switch (type)
            {
                case CellGroundType.Wall:
                    return new Color(0.56f, 0.56f, 0.60f);
                case CellGroundType.Tunnel:
                    return new Color(0.24f, 0.50f, 0.86f);
                default:
                    return Color.magenta;
            }
        }
    }
}
