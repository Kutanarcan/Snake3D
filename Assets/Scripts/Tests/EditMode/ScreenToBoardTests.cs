using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class ScreenToBoardTests
    {
        // What the scene is actually set to: Euler(30, 135, 0).
        private const float ProjectYaw = 135f;

        private const float Deadzone = 0.5f;

        private static Direction Pressing(float yaw, float screenX, float screenY)
        {
            float boardX;
            float boardY;
            new ScreenToBoard(yaw).Map(screenX, screenY, out boardX, out boardY);

            Direction direction;
            Assert.That(DirectionExtensions.TryFromVector(boardX, boardY, Deadzone, out direction), Is.True,
                "A press on its own must pick out an axis rather than landing between two.");

            return direction;
        }

        [Test]
        public void EachPressPicksOutOneGridAxis()
        {
            // The board reads as a diamond, so the grid axis that looks most like
            // "up" is the one running off the screen's top left: grid Right.
            Assert.That(Pressing(ProjectYaw, 0f, 1f), Is.EqualTo(Direction.Right));
            Assert.That(Pressing(ProjectYaw, 1f, 0f), Is.EqualTo(Direction.Down));
            Assert.That(Pressing(ProjectYaw, 0f, -1f), Is.EqualTo(Direction.Left));
            Assert.That(Pressing(ProjectYaw, -1f, 0f), Is.EqualTo(Direction.Up));
        }

        [Test]
        public void TheFourPressesNeverShareADirection()
        {
            Direction up = Pressing(ProjectYaw, 0f, 1f);
            Direction right = Pressing(ProjectYaw, 1f, 0f);
            Direction down = Pressing(ProjectYaw, 0f, -1f);
            Direction left = Pressing(ProjectYaw, -1f, 0f);

            Assert.That(new[] { up, right, down, left }, Is.Unique,
                "Four presses that collapse onto three axes leave a direction unreachable.");
        }

        [Test]
        public void OppositePressesStayOpposite()
        {
            Assert.That(Pressing(ProjectYaw, 0f, 1f).Opposite(), Is.EqualTo(Pressing(ProjectYaw, 0f, -1f)));
            Assert.That(Pressing(ProjectYaw, 1f, 0f).Opposite(), Is.EqualTo(Pressing(ProjectYaw, -1f, 0f)));
        }

        // Direction counts clockwise from Up, so turning the board a quarter is a
        // step along the enum.
        private static Direction Turned(Direction direction, int quarters)
        {
            return (Direction)(((int)direction + quarters) & 3);
        }

        [Test]
        public void TurningTheCameraAQuarterTurnTurnsTheControlsWithIt()
        {
            // Nothing is hard coded to 135: move the camera and the controls follow.
            Direction pressingUp = Pressing(ProjectYaw, 0f, 1f);

            Assert.That(Pressing(ProjectYaw + 90f, 0f, 1f), Is.EqualTo(Turned(pressingUp, 1)));
            Assert.That(Pressing(ProjectYaw + 180f, 0f, 1f), Is.EqualTo(pressingUp.Opposite()));
            Assert.That(Pressing(ProjectYaw + 270f, 0f, 1f), Is.EqualTo(Turned(pressingUp, 3)));
        }

        [Test]
        public void ACameraLeftUnturnedStillLeavesTheBoardOnItsCorner()
        {
            // At yaw 0 the half step is all that is left, so a press sits exactly
            // between two axes. This is the case the 45 exists to move away from.
            float boardX;
            float boardY;
            new ScreenToBoard(0f).Map(0f, 1f, out boardX, out boardY);

            Assert.That(boardX, Is.EqualTo(-boardY).Within(0.0001f));
        }

        [Test]
        public void TheMappingKeepsTheLengthOfWhatWasPressed()
        {
            float boardX;
            float boardY;
            new ScreenToBoard(ProjectYaw).Map(0.6f, 0.8f, out boardX, out boardY);

            Assert.That(boardX * boardX + boardY * boardY, Is.EqualTo(1f).Within(0.0001f),
                "A rotation that changed length would move the deadzone with it.");
        }
    }
}
