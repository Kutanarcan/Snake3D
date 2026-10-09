using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelSequenceTests
    {
        // Levels differ only in board width here, which is enough for a test to
        // say which one it is looking at.
        private static Level NewLevel(int width)
        {
            LevelData data = new LevelData
            {
                Width = width,
                Height = 6,
                StartX = 2,
                StartY = 2,
                StartDirection = "Right",
                SnakeLength = 3,
                FoodGoal = 5
            };

            Level level;
            Result result = Level.TryCreate(data, out level);
            Assert.That(result.IsSuccess, Is.True, result.Error);

            return level;
        }

        private static LevelSequence NewSequence(params int[] widths)
        {
            Level[] levels = new Level[widths.Length];
            for (int i = 0; i < widths.Length; i++)
            {
                levels[i] = NewLevel(widths[i]);
            }

            return new LevelSequence(levels);
        }

        [Test]
        public void ASequenceNeedsLevels()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelSequence(null));
            Assert.Throws<ArgumentException>(() => new LevelSequence(new Level[0]));
        }

        [Test]
        public void AGapInTheSequenceIsRefusedAndSaysWhere()
        {
            Level[] levels = { NewLevel(6), null, NewLevel(8) };

            ArgumentException rejection = Assert.Throws<ArgumentException>(() => new LevelSequence(levels));

            Assert.That(rejection.Message, Does.Contain("Level 1"));
        }

        [Test]
        public void ASequenceStartsOnItsFirstLevel()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            Assert.That(sequence.Index, Is.EqualTo(0));
            Assert.That(sequence.Current.Board.Width, Is.EqualTo(6));
            Assert.That(sequence.Count, Is.EqualTo(3));
        }

        [Test]
        public void AdvancingMovesToTheNextLevel()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            sequence.Advance();

            Assert.That(sequence.Index, Is.EqualTo(1));
            Assert.That(sequence.Current.Board.Width, Is.EqualTo(7));
        }

        [Test]
        public void TheLevelAfterTheLastIsTheFirstAgain()
        {
            LevelSequence sequence = NewSequence(6, 7);
            sequence.Advance();

            sequence.Advance();

            Assert.That(sequence.Index, Is.EqualTo(0));
            Assert.That(sequence.Current.Board.Width, Is.EqualTo(6));
        }

        [Test]
        public void ASequenceOfOneAdvancesOntoItself()
        {
            LevelSequence sequence = NewSequence(6);

            sequence.Advance();

            Assert.That(sequence.Index, Is.EqualTo(0));
            Assert.That(sequence.Current.Board.Width, Is.EqualTo(6));
        }

        [Test]
        public void ASequenceCanBeSentStraightToALevel()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            sequence.GoTo(2);

            Assert.That(sequence.Index, Is.EqualTo(2));
            Assert.That(sequence.Current.Board.Width, Is.EqualTo(8));
        }

        [Test]
        public void ARememberedPlacePastTheEndLandsOnTheRing()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            // What a place remembered from a build with more levels in it would say.
            sequence.GoTo(7);

            Assert.That(sequence.Index, Is.EqualTo(1));
        }

        [Test]
        public void ARememberedPlaceBeforeTheStartLandsOnTheRingToo()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            sequence.GoTo(-1);

            Assert.That(sequence.Index, Is.EqualTo(2), "C# leaves a negative remainder negative; the ring must not.");
        }

        [Test]
        public void AWholeLapReturnsToWhereItStarted()
        {
            LevelSequence sequence = NewSequence(6, 7, 8);

            for (int lap = 0; lap < 3; lap++)
            {
                sequence.Advance();
            }

            Assert.That(sequence.Index, Is.EqualTo(0));
        }
    }
}
