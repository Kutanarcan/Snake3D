using System;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class CombinedInputTests
    {
        private static InputOrchestrator Reading(params IInputSource[] sources)
        {
            return new InputOrchestrator(sources);
        }

        private static Direction Read(InputOrchestrator input)
        {
            Direction direction;
            Assert.That(input.TryRead(out direction), Is.True);

            return direction;
        }

        [Test]
        public void TheSourcesAreRequired()
        {
            Assert.Throws<ArgumentNullException>(() => new InputOrchestrator(null));
            Assert.Throws<ArgumentNullException>(() => new InputOrchestrator(new IInputSource[] { null }));
        }

        [Test]
        public void NothingIsReadWhenNoWayInHasAnything()
        {
            InputOrchestrator input = Reading(new ScriptedInputSource(), new ScriptedInputSource());

            Direction direction;
            Assert.That(input.TryRead(out direction), Is.False);
        }

        [Test]
        public void AnEmptyWayInFallsThroughToTheNext()
        {
            ScriptedInputSource keys = new ScriptedInputSource();
            InputOrchestrator input = Reading(new ScriptedInputSource(), keys);

            keys.Turn(Direction.Down);

            Assert.That(Read(input), Is.EqualTo(Direction.Down));
        }

        [Test]
        public void ATapIsNotSwallowedByAKeyThatIsBeingHeld()
        {
            // The pad is listed first on purpose: a resting thumb reports the
            // same key on every frame, and asking it first would starve the tap.
            ScriptedInputSource arrows = new ScriptedInputSource();
            ScriptedInputSource keys = new ScriptedInputSource();
            InputOrchestrator input = Reading(arrows, keys);

            arrows.Turn(Direction.Up);
            keys.Turn(Direction.Right);

            Assert.That(Read(input), Is.EqualTo(Direction.Up));
        }

        [Test]
        public void AWayInThatWasNotReachedIsLeftUnread()
        {
            ScriptedInputSource arrows = new ScriptedInputSource();
            ScriptedInputSource keys = new ScriptedInputSource();
            InputOrchestrator input = Reading(arrows, keys);

            arrows.Turn(Direction.Up);
            Read(input);

            Assert.That(keys.ReadCount, Is.EqualTo(0), "The first answer ends the question.");
        }

        [Test]
        public void ClearingForgetsEveryWayIn()
        {
            ScriptedInputSource arrows = new ScriptedInputSource();
            ScriptedInputSource keys = new ScriptedInputSource();
            InputOrchestrator input = Reading(arrows, keys);

            input.Clear();

            Assert.That(arrows.ClearCount, Is.EqualTo(1));
            Assert.That(keys.ClearCount, Is.EqualTo(1));
        }
    }
}
