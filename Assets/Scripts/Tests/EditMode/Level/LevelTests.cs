using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelTests
    {
        // A level every rule accepts. Each test spoils exactly one field, so
        // whatever it rejects, that field is the reason.
        private static LevelData Valid()
        {
            return new LevelData
            {
                Width = 10,
                Height = 10,
                StartX = 4,
                StartY = 4,
                StartDirection = "Right",
                SnakeLength = 3,
                FoodGoal = 5
            };
        }

        private static Result TryCreate(LevelData data, out Level level)
        {
            return Level.TryCreate(data, out level);
        }

        [Test]
        public void AValidLevelCarriesEveryFieldThrough()
        {
            Level level;
            Result result = TryCreate(Valid(), out level);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(level.Board.Width, Is.EqualTo(10));
            Assert.That(level.Board.Height, Is.EqualTo(10));
            Assert.That(level.SnakeStart, Is.EqualTo(new GridPosition(4, 4)));
            Assert.That(level.SnakeDirection, Is.EqualTo(Direction.Right));
            Assert.That(level.SnakeLength, Is.EqualTo(3));
            Assert.That(level.FoodGoal, Is.EqualTo(5));
        }

        [Test]
        public void ARejectedLevelHandsBackAMessageAndNothingElse()
        {
            LevelData data = Valid();
            data.Width = 0;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.Not.Null.And.Not.Empty);
            Assert.That(level, Is.Null, "A failed level must not hand back a half-built one.");
        }

        [Test]
        public void MissingDataIsRejected()
        {
            Level level;

            Assert.That(TryCreate(null, out level).IsSuccess, Is.False);
        }

        [Test]
        public void ABoardSideThatIsNotPositiveIsRejected()
        {
            LevelData wide = Valid();
            wide.Width = 0;

            LevelData tall = Valid();
            tall.Height = -3;

            Level level;
            Assert.That(TryCreate(wide, out level).IsSuccess, Is.False);
            Assert.That(TryCreate(tall, out level).IsSuccess, Is.False);
        }

        [Test]
        public void ABoardBiggerThanTheCeilingIsRejected()
        {
            LevelData data = Valid();
            data.Width = Level.MaxSide + 1;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False,
                "An unbounded side would let a typo allocate a huge cell array.");
        }

        [Test]
        public void ABoardExactlyAtTheCeilingIsAccepted()
        {
            LevelData data = Valid();
            data.Width = Level.MaxSide;
            data.Height = Level.MaxSide;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.True);
        }

        [Test]
        public void ASnakeWithoutCellsIsRejected()
        {
            LevelData data = Valid();
            data.SnakeLength = 0;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.False);
        }

        [Test]
        public void ASnakeThatFillsTheWholeBoardIsRejected()
        {
            LevelData data = Valid();
            data.Width = 2;
            data.Height = 2;
            data.StartX = 1;
            data.StartY = 1;
            data.SnakeLength = 4;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("food"), "There would be nowhere left to spawn food.");
        }

        [Test]
        public void ASnakeLyingAcrossAWallIsRejected()
        {
            LevelData data = Valid();
            data.Items = new[] { new LevelItemData { Type = "Wall", X = 3, Y = 4 } };

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False,
                "Facing Right from (4, 4), the second body cell lands on the wall at (3, 4).");
            Assert.That(result.Error, Does.Contain("Wall").And.Contain("(3, 4)"));
        }

        [Test]
        public void AFoodGoalTheBoardCannotHoldIsRejected()
        {
            LevelData data = Valid();
            data.FoodGoal = 98;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False,
                "A 3-cell snake growing by 98 needs 101 cells on a 100-cell board.");
            Assert.That(result.Error, Does.Contain("food"));
        }

        [Test]
        public void AFoodGoalThatExactlyFillsTheBoardIsAccepted()
        {
            LevelData data = Valid();
            data.FoodGoal = 97;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.True,
                "The last food is eaten and the run is won; nothing has to spawn after it.");
        }

        [Test]
        public void WallsShrinkTheRoomTheRunIsMeasuredAgainst()
        {
            // A 3x3 board walled top and bottom, leaving one open row of three.
            LevelData data = new LevelData
            {
                Width = 3,
                Height = 3,
                StartX = 2,
                StartY = 1,
                StartDirection = "Right",
                SnakeLength = 2,
                FoodGoal = 2,
                Items = new[]
                {
                    new LevelItemData { Type = "Wall", X = 0, Y = 0 },
                    new LevelItemData { Type = "Wall", X = 1, Y = 0 },
                    new LevelItemData { Type = "Wall", X = 2, Y = 0 },
                    new LevelItemData { Type = "Wall", X = 0, Y = 2 },
                    new LevelItemData { Type = "Wall", X = 1, Y = 2 },
                    new LevelItemData { Type = "Wall", X = 2, Y = 2 }
                }
            };

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False,
                "Six of the nine cells are walls, so a 2-cell snake cannot grow by 2.");
            Assert.That(result.Error, Does.Contain("food"));
        }

        [Test]
        public void ANegativeFoodGoalIsRejected()
        {
            LevelData data = Valid();
            data.FoodGoal = -1;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.False);
        }

        [Test]
        public void AFoodGoalOfZeroIsAcceptedAsAnEndlessRun()
        {
            LevelData data = Valid();
            data.FoodGoal = 0;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(level.FoodGoal, Is.EqualTo(0));
        }

        [Test]
        public void AMissingDirectionIsRejected()
        {
            LevelData missing = Valid();
            missing.StartDirection = null;

            LevelData blank = Valid();
            blank.StartDirection = "   ";

            Level level;
            Assert.That(TryCreate(missing, out level).IsSuccess, Is.False);
            Assert.That(TryCreate(blank, out level).IsSuccess, Is.False);
        }

        [Test]
        public void AnUnknownDirectionNameIsRejected()
        {
            LevelData data = Valid();
            data.StartDirection = "Sideways";

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("Sideways"), "The message should name what was wrong.");
        }

        [Test]
        public void ADirectionWrittenAsANumberIsRejected()
        {
            LevelData data = Valid();
            data.StartDirection = "7";

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.False,
                "Enum.TryParse accepts stray numbers, so a defined name is required.");
        }

        [Test]
        public void DirectionNamesAreReadWithoutRegardToCase()
        {
            LevelData data = Valid();
            data.StartDirection = "dOwN";
            data.StartY = 6;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(level.SnakeDirection, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void AHeadOffTheBoardIsRejected()
        {
            LevelData data = Valid();
            data.StartX = 10;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.False);
        }

        [Test]
        public void ATailThatWouldHangOffTheBoardIsRejected()
        {
            LevelData data = Valid();
            data.StartX = 1;
            data.StartY = 5;
            data.SnakeLength = 3;

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False,
                "Facing Right from x=1, a 3-cell body reaches back to x=-1.");
            Assert.That(result.Error, Does.Contain("tail"));
        }

        [Test]
        public void ABodyThatEndsOnTheLastColumnIsAccepted()
        {
            LevelData data = Valid();
            data.StartX = 2;
            data.StartY = 5;
            data.SnakeLength = 3;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.True,
                "Facing Right from x=2, the tail lands exactly on x=0.");
        }

        [Test]
        public void ASingleCellSnakeOnlyNeedsItsHeadToFit()
        {
            LevelData data = Valid();
            data.StartX = 0;
            data.StartY = 0;
            data.SnakeLength = 1;

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.True);
        }

        [Test]
        public void ALevelWithoutAGoldenAppleBlockNeverGetsOne()
        {
            Level level;

            Assert.That(TryCreate(Valid(), out level).IsSuccess, Is.True);
            Assert.That(level.GoldenApple.IsEnabled, Is.False);
        }

        [Test]
        public void TheGoldenAppleCooldownsAreCarriedThroughInSeconds()
        {
            LevelData data = Valid();
            data.GoldenApple = new GoldenAppleData { SpawnAfterSeconds = 6f, StaysForSeconds = 4f };

            Level level;

            Assert.That(TryCreate(data, out level).IsSuccess, Is.True);
            Assert.That(level.GoldenApple.IsEnabled, Is.True);
            Assert.That(level.GoldenApple.SpawnAfterSeconds, Is.EqualTo(6f).Within(0.001f));
            Assert.That(level.GoldenApple.StaysForSeconds, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void ASecondsCooldownBecomesAWholeNumberOfSteps()
        {
            GoldenAppleRule rule = new GoldenAppleRule(6f, 0.05f);

            Assert.That(rule.SpawnTicks(0.15f), Is.EqualTo(40));
            Assert.That(rule.StayTicks(0.15f), Is.EqualTo(1),
                "A cooldown shorter than a tick still costs a whole one.");
        }

        [Test]
        public void AGoldenAppleBlockWithAZeroCooldownIsRejected()
        {
            LevelData data = Valid();
            data.GoldenApple = new GoldenAppleData { SpawnAfterSeconds = 6f, StaysForSeconds = 0f };

            Level level;
            Result result = TryCreate(data, out level);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Does.Contain("golden apple"));
            Assert.That(level, Is.Null);
        }
    }
}
