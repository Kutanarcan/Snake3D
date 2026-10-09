using Game.Authoring;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelDraftTests
    {
        private static LevelData Valid()
        {
            return new LevelData
            {
                Width = 9,
                Height = 9,
                StartX = 4,
                StartY = 4,
                StartDirection = "Right",
                SnakeLength = 3,
                FoodGoal = 7,
                Items = new[]
                {
                    new LevelItemData { Type = "Wall", X = 0, Y = 0 },
                    new LevelItemData { Type = "Tunnel", X = 6, Y = 2 }
                },
                GoldenApple = new GoldenAppleData { SpawnAfterSeconds = 8f, StaysForSeconds = 5f }
            };
        }

        [Test]
        public void ANewDraftIsAlreadyPlayable()
        {
            LevelDraft draft = new LevelDraft(new GridSize(10, 10));

            Assert.That(draft.Validate().IsSuccess, Is.True,
                "A designer opening the tool should not be met with an error.");
        }

        [Test]
        public void AFileSurvivesBeingOpenedAndSavedAgain()
        {
            LevelData data = LevelDraft.From(Valid()).ToData();
            LevelData before = Valid();

            Assert.That(data.Width, Is.EqualTo(before.Width));
            Assert.That(data.Height, Is.EqualTo(before.Height));
            Assert.That(data.StartX, Is.EqualTo(before.StartX));
            Assert.That(data.StartY, Is.EqualTo(before.StartY));
            Assert.That(data.StartDirection, Is.EqualTo("Right"));
            Assert.That(data.SnakeLength, Is.EqualTo(before.SnakeLength));
            Assert.That(data.FoodGoal, Is.EqualTo(before.FoodGoal));
            Assert.That(data.Items.Length, Is.EqualTo(2));
            Assert.That(data.GoldenApple.SpawnAfterSeconds, Is.EqualTo(8f).Within(0.001f));
        }

        [Test]
        public void ARoundTripLeavesEveryItemOnItsOwnCell()
        {
            LevelDraft draft = LevelDraft.From(Valid());

            Level level;
            Result result = Level.TryCreate(draft.ToData(), out level);

            Assert.That(result.IsSuccess, Is.True, result.Error);
            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(level.GroundAt(new GridPosition(6, 2)), Is.EqualTo(CellGroundType.Tunnel));
            Assert.That(level.GroundAt(new GridPosition(1, 1)), Is.EqualTo(CellGroundType.Open));
        }

        [Test]
        public void PaintingAndErasingShowUpInWhatIsSaved()
        {
            LevelDraft draft = LevelDraft.From(Valid());

            draft.Items.Paint(new GridPosition(8, 8), CellGroundType.Wall);
            draft.Items.Erase(new GridPosition(0, 0));

            Level level;
            Assert.That(Level.TryCreate(draft.ToData(), out level).IsSuccess, Is.True);
            Assert.That(level.GroundAt(new GridPosition(8, 8)), Is.EqualTo(CellGroundType.Wall));
            Assert.That(level.GroundAt(new GridPosition(0, 0)), Is.EqualTo(CellGroundType.Open));
        }

        [Test]
        public void ADraftReportsTheSameComplaintTheGameWould()
        {
            LevelDraft draft = LevelDraft.From(Valid());
            draft.SnakeLength = 40;

            Result fromTheDraft = draft.Validate();

            Level level;
            Result fromTheGame = Level.TryCreate(draft.ToData(), out level);

            Assert.That(fromTheDraft.IsSuccess, Is.False);
            Assert.That(fromTheDraft.Error, Is.EqualTo(fromTheGame.Error),
                "The tool must not grow rules of its own.");
        }

        [Test]
        public void ASnakeThatNoLongerFitsIsReportedRatherThanRefused()
        {
            LevelDraft draft = new LevelDraft(new GridSize(10, 10));

            draft.Start = new GridPosition(0, 0);

            Assert.That(draft.Validate().IsSuccess, Is.False, "The tail hangs off the left edge.");
            Assert.That(draft.Start, Is.EqualTo(new GridPosition(0, 0)), "Editing is allowed to be wrong.");
        }

        [Test]
        public void AnUnreadableDirectionOpensAsUpAndFailsValidationLoudly()
        {
            LevelData data = Valid();
            data.StartDirection = "Sideways";

            LevelDraft draft = LevelDraft.From(data);

            Assert.That(draft.StartDirection, Is.EqualTo(Direction.Up));
            Assert.That(draft.ToData().StartDirection, Is.EqualTo("Up"));
        }

        [Test]
        public void ShrinkingTheBoardIsCarriedIntoWhatIsSaved()
        {
            LevelDraft draft = LevelDraft.From(Valid());

            draft.Resize(new GridSize(5, 5));

            LevelData data = draft.ToData();

            Assert.That(data.Width, Is.EqualTo(5));
            Assert.That(data.Items.Length, Is.EqualTo(1), "The tunnel at (6,2) is off the smaller board.");
        }
    }
}
