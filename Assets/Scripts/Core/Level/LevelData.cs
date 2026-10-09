namespace Game.Core
{
    public sealed class LevelData
    {
        public int Width;
        public int Height;
        public int StartX;
        public int StartY;
        public string StartDirection;
        public int SnakeLength;
        public int FoodGoal;
        public LevelItemData[] Items;
        public GoldenAppleData GoldenApple;
    }
}
