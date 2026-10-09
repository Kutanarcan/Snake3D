namespace Game.Core
{
    // One thing a level puts on the board: what it is and where it goes.
    // Types that carry their own settings will add them here; the ones there
    // are today need nothing but a name and a cell.
    public sealed class LevelItemData
    {
        public string Type;
        public int X;
        public int Y;
    }
}
