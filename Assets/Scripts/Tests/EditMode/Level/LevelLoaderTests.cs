using Game.Core;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelLoaderTests
    {
        private const string ValidFile = @"{
            ""Width"": 16,
            ""Height"": 16,
            ""StartX"": 8,
            ""StartY"": 8,
            ""StartDirection"": ""Right"",
            ""SnakeLength"": 3,
            ""FoodGoal"": 10
        }";

        [Test]
        public void AValidFileBecomesALevel()
        {
            Level level;
            Result result = LevelLoader.TryLoad(ValidFile, out level);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(level.Board.Width, Is.EqualTo(16));
            Assert.That(level.Board.Height, Is.EqualTo(16));
            Assert.That(level.SnakeStart, Is.EqualTo(new GridPosition(8, 8)));
            Assert.That(level.SnakeDirection, Is.EqualTo(Direction.Right));
            Assert.That(level.SnakeLength, Is.EqualTo(3));
            Assert.That(level.FoodGoal, Is.EqualTo(10));
        }

        [Test]
        public void TextThatIsNotJsonComesBackAsAMessageRatherThanAnException()
        {
            Level level;
            Result result = LevelLoader.TryLoad("{ this is not json", out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("JSON"));
            Assert.That(level, Is.Null);
        }

        [Test]
        public void AFieldOfTheWrongTypeComesBackAsAMessageRatherThanAnException()
        {
            Level level;
            Result result = LevelLoader.TryLoad(@"{ ""Width"": ""wide"" }", out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(level, Is.Null);
        }

        [Test]
        public void AnEmptyFileIsRejected()
        {
            Level level;

            Assert.That(LevelLoader.TryLoad(null, out level).IsSuccess, Is.False);
            Assert.That(LevelLoader.TryLoad("", out level).IsSuccess, Is.False);
            Assert.That(LevelLoader.TryLoad("   \n  ", out level).IsSuccess, Is.False);
        }

        [Test]
        public void AFileHoldingNothingButNullIsRejected()
        {
            Level level;
            Result result = LevelLoader.TryLoad("null", out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(level, Is.Null);
        }

        [Test]
        public void MissingFieldsFallToTheirDefaultsAndAreCaughtByTheRules()
        {
            Level level;
            Result result = LevelLoader.TryLoad("{}", out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("wide"),
                "An absent Width deserializes to 0, which the board rule rejects.");
        }

        [Test]
        public void AFieldTheGameDoesNotKnowYetIsIgnored()
        {
            string fromALaterVersion = ValidFile.Insert(1, @"""Walls"": [3, 4, 5],");

            Level level;
            Result result = LevelLoader.TryLoad(fromALaterVersion, out level);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(level.Board.Width, Is.EqualTo(16),
                "Adding a field later must not break the files that already exist.");
        }

        [Test]
        public void ABadValueIsReportedByTheSameRuleThatGuardsTheRestOfTheGame()
        {
            Level level;
            Result result = LevelLoader.TryLoad(
                ValidFile.Replace(@"""StartDirection"": ""Right""", @"""StartDirection"": ""Sideways"""), out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Sideways"),
                "The loader adds no rules of its own; Level.TryCreate is still the only gate.");
        }
    }
}
