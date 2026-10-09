using System;

namespace Game.Core
{
    internal static class GroundRules
    {
        internal static bool Kills(CellGroundType ground)
        {
            switch (ground)
            {
                case CellGroundType.Open:
                case CellGroundType.Tunnel:
                    return false;
                case CellGroundType.Wall:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ground));
            }
        }

        internal static bool Deflects(CellGroundType ground)
        {
            return ground == CellGroundType.Tunnel;
        }
    }
}
