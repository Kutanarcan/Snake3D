using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class DirectionTests
    {
        [TestCase(Direction.Up, Direction.Down)]
        [TestCase(Direction.Down, Direction.Up)]
        [TestCase(Direction.Left, Direction.Right)]
        [TestCase(Direction.Right, Direction.Left)]
        public void Opposite_ReturnsTheReverseDirection(Direction direction, Direction expected)
        {
            Assert.That(direction.Opposite(), Is.EqualTo(expected));
            Assert.That(direction.IsOpposite(expected), Is.True);
        }

        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Right, 1, 0)]
        [TestCase(Direction.Down, 0, -1)]
        [TestCase(Direction.Left, -1, 0)]
        public void ToOffset_MatchesABottomLeftOrigin(Direction direction, int expectedX, int expectedY)
        {
            Assert.That(direction.ToOffset(), Is.EqualTo(new GridPosition(expectedX, expectedY)));
        }

        [Test]
        public void Opposite_IsItsOwnInverse()
        {
            foreach (Direction direction in new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left })
            {
                Assert.That(direction.Opposite().Opposite(), Is.EqualTo(direction));
            }
        }

        [TestCase(1f, 0f, Direction.Right)]
        [TestCase(-1f, 0f, Direction.Left)]
        [TestCase(0f, 1f, Direction.Up)]
        [TestCase(0f, -1f, Direction.Down)]
        [TestCase(0.9f, 0.4f, Direction.Right)]
        [TestCase(-0.3f, -0.8f, Direction.Down)]
        public void TryFromVector_TheStrongerAxisWins(float x, float y, Direction expected)
        {
            Direction direction;

            Assert.That(DirectionExtensions.TryFromVector(x, y, 0.2f, out direction), Is.True);
            Assert.That(direction, Is.EqualTo(expected));
        }

        [TestCase(0f, 0f)]
        [TestCase(0.1f, 0.1f)]
        [TestCase(-0.19f, 0.19f)]
        public void TryFromVector_BelowTheDeadzoneAsksForNothing(float x, float y)
        {
            Direction direction;

            Assert.That(DirectionExtensions.TryFromVector(x, y, 0.2f, out direction), Is.False);
        }

        [TestCase(0.7f, 0.7f)]
        [TestCase(-1f, 1f)]
        [TestCase(1f, -1f)]
        public void TryFromVector_APerfectDiagonalAsksForNothing(float x, float y)
        {
            Direction direction;

            Assert.That(DirectionExtensions.TryFromVector(x, y, 0.2f, out direction), Is.False,
                "A snake has no diagonal, so picking an axis here would be arbitrary.");
        }

        [TestCase(Direction.Up, Direction.Right)]
        [TestCase(Direction.Right, Direction.Up)]
        [TestCase(Direction.Down, Direction.Left)]
        [TestCase(Direction.Left, Direction.Down)]
        public void DeflectMirrorsTheHeadingAcrossTheDiagonal(Direction heading, Direction expected)
        {
            Assert.That(heading.Deflect(), Is.EqualTo(expected), "(x, y) has to come back as (y, x).");
        }

        [TestCase(Direction.Up)]
        [TestCase(Direction.Right)]
        [TestCase(Direction.Down)]
        [TestCase(Direction.Left)]
        public void DeflectIsItsOwnInverse(Direction heading)
        {
            Assert.That(heading.Deflect().Deflect(), Is.EqualTo(heading),
                "Two tunnels in a row leave the snake going the way it came in.");
        }

        [TestCase(Direction.Up)]
        [TestCase(Direction.Right)]
        [TestCase(Direction.Down)]
        [TestCase(Direction.Left)]
        public void DeflectNeverTurnsTheSnakeBackOnItself(Direction heading)
        {
            Assert.That(heading.Deflect(), Is.Not.EqualTo(heading.Opposite()),
                "A tunnel that reversed the head would fold it into its own neck.");
        }
    }
}
