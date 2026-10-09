namespace Game.Core
{
    // What is standing on a cell right now. Pieces arrive and leave; the ground
    // underneath them outlives them.
    public enum CellOccupantType : byte
    {
        Empty = 0,
        Snake = 1,
        Food = 2,
        GoldenApple = 3
    }
}
