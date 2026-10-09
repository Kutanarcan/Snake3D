using System;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class BoardFitTests
    {
        private const float Tolerance = 0.001f;
        private const float Portrait = 1080f / 1920f;

        private static readonly Quaternion TopDown = Quaternion.Euler(90f, 0f, 0f);
        private static readonly Quaternion Shot = Quaternion.Euler(30f, 135f, 0f);

        private static BoardFit Fit(int width, int height, Quaternion rotation, float aspect,
            float padding, float top = 0f, float bottom = 0f)
        {
            return BoardFit.For(new GridSize(width, height), rotation, aspect, padding, top, bottom);
        }

        [Test]
        public void TheShotTheCameraWasHandTunedToIsWhatComesOut()
        {
            BoardFit fit = Fit(15, 15, Shot, Portrait, 0.5f, 0.1f, 0.25f);

            // Turned 135 degrees the board is seen corner on, so its 8 by 8
            // half-sides fall on the camera's right axis as 2 * 8 * cos(45) =
            // 11.3137, and a 0.5625 wide screen needs 11.3137 / 0.5625 to show
            // it. The 30 degree tilt leaves only 5.6569 going up the screen,
            // which the panels stretch to 8.7029 -- not the binding side.
            Assert.That(fit.OrthographicSize, Is.EqualTo(20.1133f).Within(Tolerance));
            Assert.That(fit.UpShift, Is.EqualTo(3.0170f).Within(Tolerance));
            Assert.That(fit.Depth, Is.EqualTo(9.7980f).Within(Tolerance));
            Assert.That(fit.CenterX, Is.EqualTo(7f).Within(Tolerance));
            Assert.That(fit.CenterZ, Is.EqualTo(7f).Within(Tolerance));
        }

        [Test]
        public void FromStraightAboveTheBoardIsExactlyItsOwnHalfSide()
        {
            BoardFit fit = Fit(16, 16, TopDown, 1f, 0f);

            Assert.That(fit.OrthographicSize, Is.EqualTo(8f).Within(Tolerance));
            Assert.That(fit.Depth, Is.EqualTo(0f).Within(Tolerance),
                "A flat board seen from straight above reaches nowhere towards the camera.");
        }

        [Test]
        public void ATiltedCameraGivesTheFlatBoardDepth()
        {
            Assert.That(Fit(16, 16, Shot, 1f, 0f).Depth, Is.GreaterThan(0f));
        }

        [Test]
        public void AQuarterTurnSwapsWhichSideRunsOutOfScreenFirst()
        {
            // A 20 by 10 board on a screen twice as wide as it is tall: facing
            // it square the height binds, turned a quarter the width does.
            Assert.That(Fit(20, 10, TopDown, 2f, 0f).OrthographicSize,
                Is.EqualTo(5f).Within(Tolerance));

            Assert.That(Fit(20, 10, Quaternion.Euler(90f, 90f, 0f), 2f, 0f).OrthographicSize,
                Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void ANarrowScreenWidensTheShot()
        {
            Assert.That(Fit(16, 16, TopDown, 0.5f, 0f).OrthographicSize,
                Is.EqualTo(16f).Within(Tolerance));
        }

        [Test]
        public void PanelsWidenTheShotByWhatTheyCover()
        {
            // Half the height left over doubles what the camera has to show.
            Assert.That(Fit(16, 16, TopDown, 1f, 0f, 0.25f, 0.25f).OrthographicSize,
                Is.EqualTo(16f).Within(Tolerance));
        }

        [Test]
        public void PanelsOfEqualDepthLeaveTheCameraCentred()
        {
            Assert.That(Fit(16, 16, TopDown, 1f, 0f, 0.25f, 0.25f).UpShift,
                Is.EqualTo(0f).Within(Tolerance));

            Assert.That(Fit(16, 16, TopDown, 1f, 0f).UpShift, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void TheDeeperPanelPullsTheCameraTowardsIt()
        {
            // A deeper pad below leaves the free band's middle above the middle
            // of the screen, so the camera drops to put the board there.
            Assert.That(Fit(16, 16, TopDown, 1f, 0f, 0.1f, 0.3f).UpShift, Is.GreaterThan(0f));
            Assert.That(Fit(16, 16, TopDown, 1f, 0f, 0.3f, 0.1f).UpShift, Is.LessThan(0f));
        }

        [Test]
        public void PaddingWidensTheShot()
        {
            Assert.That(Fit(16, 16, TopDown, 1f, 1f).OrthographicSize,
                Is.EqualTo(9f).Within(Tolerance));
        }

        [Test]
        public void TheCentreIsTheMiddleOfTheCellsRatherThanOfTheArea()
        {
            BoardFit oblong = Fit(5, 9, TopDown, 1f, 0f);
            Assert.That(oblong.CenterX, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(oblong.CenterZ, Is.EqualTo(4f).Within(Tolerance));

            BoardFit single = Fit(1, 1, TopDown, 1f, 0f);
            Assert.That(single.CenterX, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(single.CenterZ, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void AViewportWithNoWidthOrNoHeightIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Fit(8, 8, TopDown, 0f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Fit(8, 8, TopDown, -1f, 0f));
        }

        [Test]
        public void PaddingThatPullsInwardsIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Fit(8, 8, TopDown, 1f, -0.1f));
        }

        [Test]
        public void PanelsThatFillTheScreenAreRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Fit(8, 8, TopDown, 1f, 0f, 0.5f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Fit(8, 8, TopDown, 1f, 0f, 0.7f, 0.4f));
        }
    }
}
