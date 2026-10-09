using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class RunEndingsTests
    {
        private static readonly GridSize Board = new GridSize(6, 6);

        private static SnakeGame NewGame()
        {
            GridMap map = new GridMap(Board);
            IRandomSource random = new PinnedRandomSource(map, new GridPosition(0, 0));

            return new SnakeGame(map, new Snake(Board.CellCount),
                new AppleSpawnManager(new FoodSpawner(map, random), new GoldenAppleSpawner(map, random, 0, 0)));
        }

        [Test]
        public void HittingAWallIsAWallHit()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(2, 2), Direction.Right, 3);
            game.Map.SetGround(new GridPosition(3, 2), CellGroundType.Wall);
            game.Step();

            Assert.That(RunEndings.For(game), Is.EqualTo(EndingId.WallHit));
        }

        [Test]
        public void WalkingOffTheBoardIsAnEndingOfItsOwn()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(5, 2), Direction.Right, 3);
            game.Step();

            Assert.That(RunEndings.For(game), Is.EqualTo(EndingId.OutOfBounds));
        }

        [Test]
        public void BitingItsOwnBodyIsAnEndingOfItsOwn()
        {
            SnakeGame game = NewGame();
            game.Reset(new GridPosition(4, 2), Direction.Right, 5);

            game.TryTurn(Direction.Up);
            game.Step();
            game.TryTurn(Direction.Left);
            game.Step();
            game.TryTurn(Direction.Down);
            game.Step();

            Assert.That(RunEndings.For(game), Is.EqualTo(EndingId.SelfBite));
        }
    }
}
