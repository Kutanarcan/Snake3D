using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class SafeAreaFitTests
    {
        private const int Width = 1080;
        private const int Height = 1920;

        [Test]
        public void AScreenWithNothingInTheWayIsUsedWhole()
        {
            SafeAreaFit fit = SafeAreaFit.For(new Rect(0f, 0f, Width, Height), Width, Height);

            Assert.That(fit.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(fit.AnchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void ANotchAtTheTopPullsTheTopAnchorDown()
        {
            // 132 pixels of notch and 63 of home indicator, the shape a modern
            // phone reports when it is held upright.
            SafeAreaFit fit = SafeAreaFit.For(new Rect(0f, 63f, Width, Height - 132f - 63f), Width, Height);

            Assert.That(fit.AnchorMin.y, Is.EqualTo(63f / Height).Within(0.0001f));
            Assert.That(fit.AnchorMax.y, Is.EqualTo((Height - 132f) / Height).Within(0.0001f));
            Assert.That(fit.AnchorMin.x, Is.EqualTo(0f));
            Assert.That(fit.AnchorMax.x, Is.EqualTo(1f));
        }

        [Test]
        public void ACutoutOnTheSideNarrowsTheAnchorsSideways()
        {
            SafeAreaFit fit = SafeAreaFit.For(new Rect(90f, 0f, Width - 180f, Height), Width, Height);

            Assert.That(fit.AnchorMin.x, Is.EqualTo(90f / Width).Within(0.0001f));
            Assert.That(fit.AnchorMax.x, Is.EqualTo((Width - 90f) / Width).Within(0.0001f));
        }

        [Test]
        public void AScreenWithNoSizeYetIsUsedWhole()
        {
            SafeAreaFit fit = SafeAreaFit.For(new Rect(0f, 0f, 0f, 0f), 0, 0);

            Assert.That(fit.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(fit.AnchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void ASafeAreaWithNoRoomInItIsIgnored()
        {
            SafeAreaFit fit = SafeAreaFit.For(new Rect(500f, 500f, 0f, 0f), Width, Height);

            Assert.That(fit.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(fit.AnchorMax, Is.EqualTo(Vector2.one), "A safe area of nothing is worse than all of it.");
        }

        [Test]
        public void ASafeAreaReachingPastTheScreenStopsAtTheEdge()
        {
            SafeAreaFit fit = SafeAreaFit.For(new Rect(-40f, -40f, Width + 200f, Height + 200f), Width, Height);

            Assert.That(fit.AnchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(fit.AnchorMax, Is.EqualTo(Vector2.one));
        }
    }
}
