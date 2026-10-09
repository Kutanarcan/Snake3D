namespace Game.Core
{
    // What a cell is made of. The ground is laid out when the level is built
    // and stays where it was put while pieces move across it.
    public enum CellGroundType : byte
    {
        Open = 0,
        Wall = 1,
        Tunnel = 2
    }
}
