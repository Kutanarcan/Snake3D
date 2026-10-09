using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class StepEaseTests
    {
        private const float Nudge = 0.0005f;

        // How fast the step is moving at t, read off the curve rather than
        // derived, so the test would still catch a curve that was swapped out.
        private static float SpeedAt(float amount, float t)
        {
            return (StepEase.Apply(t + Nudge, amount) - StepEase.Apply(t - Nudge, amount)) / (2f * Nudge);
        }

        [Test]
        public void NoEaseIsTheFlatSlideItReplaced([Range(0f, 1f, 0.05f)] float t)
        {
            Assert.That(StepEase.Apply(t, 0f), Is.EqualTo(t).Within(0.0001f));
        }

        [Test]
        public void TheStepStillLandsWhenTheTickLands([Range(0f, 1f, 0.25f)] float amount)
        {
            Assert.That(StepEase.Apply(0f, amount), Is.EqualTo(0f).Within(0.0001f), "The step must leave from where it is.");
            Assert.That(StepEase.Apply(1f, amount), Is.EqualTo(1f).Within(0.0001f), "Easing spends the tick differently, it does not lengthen it.");
        }

        [Test]
        public void TheStepNeverRunsBackwards([Range(0f, 1f, 0.25f)] float amount)
        {
            float previous = StepEase.Apply(0f, amount);

            for (int i = 1; i <= 200; i++)
            {
                float current = StepEase.Apply(i / 200f, amount);

                Assert.That(current, Is.GreaterThanOrEqualTo(previous), "The snake slid back towards the cell it left.");
                previous = current;
            }
        }

        [Test]
        public void TheStepAcceleratesOutOfACellAndDeceleratesIntoTheNext()
        {
            float leaving = SpeedAt(0.5f, 0.05f);
            float middle = SpeedAt(0.5f, 0.5f);
            float arriving = SpeedAt(0.5f, 0.95f);

            Assert.That(middle, Is.GreaterThan(leaving), "The step should be quickest between the two cells.");
            Assert.That(middle, Is.GreaterThan(arriving), "The step should be settling by the time it arrives.");
        }

        [Test]
        public void BothEndsRunAtTheSameSpeedSoTheSeamBetweenTicksHasNoJumpInIt()
        {
            // The tick boundary hands t = 1 straight back as t = 0, so a mismatch
            // here is a visible hitch on every single step.
            Assert.That(SpeedAt(0.5f, Nudge * 2f), Is.EqualTo(SpeedAt(0.5f, 1f - Nudge * 2f)).Within(0.01f));
        }

        [Test]
        public void TheAmountSaysHowMuchOfTheCellSpeedIsGivenUp()
        {
            // f'(0) = f'(1) = 1 - a is what the knob is actually promising.
            Assert.That(SpeedAt(0.25f, Nudge * 2f), Is.EqualTo(0.75f).Within(0.01f));
            Assert.That(SpeedAt(1f, Nudge * 2f), Is.EqualTo(0f).Within(0.01f), "A full ease comes to a stop on every cell.");
        }

        [Test]
        public void ATickThatOverranIsHeldOnItsCell()
        {
            // A run that ends part way through a tick leaves the timer holding
            // more than one tick, and the body must not drift past the board.
            Assert.That(StepEase.Apply(2.5f, 0.5f), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(StepEase.Apply(-0.5f, 0.5f), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void AnAmountOutsideItsRangeCannotTurnTheStepAround()
        {
            Assert.That(StepEase.Apply(0.5f, 5f), Is.EqualTo(StepEase.Apply(0.5f, 1f)).Within(0.0001f));
            Assert.That(StepEase.Apply(0.5f, -5f), Is.EqualTo(0.5f).Within(0.0001f));
        }
    }
}
