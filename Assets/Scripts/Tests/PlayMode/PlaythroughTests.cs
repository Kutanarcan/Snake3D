using System;
using System.Collections.Generic;
using Game.Core;
using Game.Input;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.PlayMode
{
    public sealed class PlaythroughTests
    {
        // A corridor one cell tall with a goal of one apple. Wherever the apple
        // lands the snake walks onto it before it can reach the far wall, so the
        // level is won without the test having to steer or pin anything.
        private const string Corridor3 = @"{
            ""Width"": 3, ""Height"": 1, ""StartX"": 0, ""StartY"": 0,
            ""StartDirection"": ""Right"", ""SnakeLength"": 1, ""FoodGoal"": 1 }";

        private const string Corridor4 = @"{
            ""Width"": 4, ""Height"": 1, ""StartX"": 0, ""StartY"": 0,
            ""StartDirection"": ""Right"", ""SnakeLength"": 1, ""FoodGoal"": 1 }";

        // Standing in for PlayerPrefs, so a test can say where the player left off
        // and read back what the playthrough recorded.
        private sealed class RememberedLevel : ILevelPersistent
        {
            public int Value;
            public int Writes;

            public int Read()
            {
                return Value;
            }

            public void Write(int index)
            {
                Value = index;
                Writes++;
            }
        }

        private GameRig _rig;
        private BoardView _board;
        private Player_Input _actions;
        private RememberedLevel _levelPersistent;
        private ArrowInput _arrows;
        private Playthrough _playthrough;

        [SetUp]
        public void SetUp()
        {
            _rig = new GameRig(GameRig.ValidLevel);
            _actions = new Player_Input();
            _levelPersistent = new RememberedLevel();
        }

        [TearDown]
        public void TearDown()
        {
            _actions.Dispose();
            _rig.Destroy();
        }

        // Stocked from the sequence exactly the way the installer does it, so a
        // level change here draws on the same cells the game would.
        private void Play(params string[] jsons)
        {
            LevelSequence levels = NewSequence(jsons);
            CellStock stock = CellStock.For(levels);

            CellViewPool environment = new CellViewPool(_rig.CellPrefab, _rig.Host.transform,
                stock.Environment + stock.Ground);
            _board = new BoardView(environment, _rig.NewRoot("board"), _rig.Visuals);
            GroundView ground = new GroundView(environment, _rig.NewRoot("ground"), _rig.Visuals);

            CellViewPool organic = new CellViewPool(_rig.CellPrefab, _rig.Host.transform, stock.Organic);
            SnakeView snake = new SnakeView(organic, _rig.NewRoot("snake"), _rig.Visuals);
            AppleView food = _rig.NewApple(organic, "food");
            AppleView golden = _rig.NewGoldenApple(organic, "golden");

            GameRun run = new GameRun(_actions, new RunPieces(snake, food, golden, _rig.Audio, NewEffects()),
                _rig.Information, _rig.Popup, _rig.Audio, NewTimeline());
            _arrows = new ArrowInput();

            _playthrough = new Playthrough(levels, run,
                new BoardPresentation(_board, ground, _rig.Framing), _rig.Popup, GameRig.Interval,
                new SystemRandomSource(1), _arrows, _levelPersistent, _rig.Audio);

            _playthrough.Begin();
        }

        private EffectPlayer NewEffects()
        {
            return new EffectPlayer(_rig.Effects, _rig.NewRoot("effects"), 1f);
        }

        private RunTimeline NewTimeline()
        {
            return new RunTimeline(_rig.Endings, _rig.Audio, _rig.Shaker, _rig.Host);
        }

        private static LevelSequence NewSequence(string[] jsons)
        {
            List<Level> levels = new List<Level>();

            foreach (string json in jsons)
            {
                Level level;
                Result loadResult = LevelLoader.TryLoad(json, out level);
                Assert.That(loadResult.IsSuccess, Is.True, loadResult.Error);

                levels.Add(level);
            }

            return new LevelSequence(levels.ToArray());
        }

        // Every level waits for the player before it moves, so each one has to be
        // set off. The corridors all face Right, so this steers nothing.
        private void PlayUntilTheLevelIsOver()
        {
            _arrows.Press(Direction.Right);

            for (int frame = 0; frame < 64 && !_playthrough.Session.IsOver; frame++)
            {
                _playthrough.Tick(GameRig.Interval);
            }

            Assert.That(_playthrough.Session.IsOver, Is.True, "The level never ended.");
        }

        private int BoardWidth
        {
            get { return _playthrough.Session.Game.Map.Size.Width; }
        }

        private int DrawnGroundCells
        {
            get { return _board.Root.GetComponentsInChildren<CellView>(false).Length; }
        }

        [Test]
        public void EverythingAPlaythroughRunsOnIsRequired()
        {
            LevelSequence levels = NewSequence(new[] { Corridor3 });
            CellViewPool pool = new CellViewPool(_rig.CellPrefab, _rig.Host.transform, 64);

            BoardPresentation board = new BoardPresentation(
                new BoardView(pool, _rig.NewRoot("board"), _rig.Visuals),
                new GroundView(pool, _rig.NewRoot("ground"), _rig.Visuals),
                _rig.Framing);

            GameRun run = new GameRun(_actions,
                new RunPieces(
                    new SnakeView(pool, _rig.NewRoot("snake"), _rig.Visuals),
                    _rig.NewApple(pool, "food"),
                    _rig.NewGoldenApple(pool, "golden"), _rig.Audio, NewEffects()),
                _rig.Information, _rig.Popup, _rig.Audio, NewTimeline());

            IRandomSource random = new SystemRandomSource(1);
            IInputSource input = new ArrowInput();

            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(null, run, board, _rig.Popup, GameRig.Interval, random, input, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, null, board, _rig.Popup, GameRig.Interval, random, input, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, null, _rig.Popup, GameRig.Interval, random, input, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, board, null, GameRig.Interval, random, input, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, board, _rig.Popup, GameRig.Interval, null, input, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, board, _rig.Popup, GameRig.Interval, random, null, _levelPersistent, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, board, _rig.Popup, GameRig.Interval, random, input, null, _rig.Audio));
            Assert.Throws<ArgumentNullException>(
                () => new Playthrough(levels, run, board, _rig.Popup, GameRig.Interval, random, input, _levelPersistent, null));
        }

        [Test]
        public void APlaythroughPicksUpWhereThePlayerLeftOff()
        {
            _levelPersistent.Value = 1;

            Play(Corridor3, Corridor4);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(1));
            Assert.That(BoardWidth, Is.EqualTo(4));
        }

        [Test]
        public void MovingOnRecordsTheLevelThePlayerReached()
        {
            Play(Corridor3, Corridor4);
            PlayUntilTheLevelIsOver();

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_levelPersistent.Value, Is.EqualTo(1));
            Assert.That(_levelPersistent.Writes, Is.EqualTo(1));
        }

        [Test]
        public void PlayingTheSameLevelAgainRecordsNothing()
        {
            Play(GameRig.ValidLevel, Corridor4);
            PlayUntilTheLevelIsOver();

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_levelPersistent.Writes, Is.EqualTo(0), "Where the player is has not changed.");
        }

        [Test]
        public void APlaceRememberedFromALongerSequenceStillLands()
        {
            _levelPersistent.Value = 5;

            Play(Corridor3, Corridor4);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(1), "Five levels on from nowhere is level two of two.");
        }

        [Test]
        public void APlaythroughStartsOnTheFirstLevelOfTheSequence()
        {
            Play(Corridor3, Corridor4);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(0));
            Assert.That(BoardWidth, Is.EqualTo(3));
            Assert.That(DrawnGroundCells, Is.EqualTo(3), "The ground is laid out for the level being played.");
            Assert.That(_playthrough.Session.Status, Is.EqualTo(GameStatus.Ready), "Every level waits for the player.");
        }

        [Test]
        public void WinningAndAcceptingTheEndingOpensTheNextLevel()
        {
            Play(Corridor3, Corridor4);
            PlayUntilTheLevelIsOver();
            Assert.That(_playthrough.Session.Status, Is.EqualTo(GameStatus.Won));

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(1));
            Assert.That(BoardWidth, Is.EqualTo(4), "The next level brought its own board.");
            Assert.That(DrawnGroundCells, Is.EqualTo(4), "The ground was laid out again for it.");
            Assert.That(_playthrough.Session.Status, Is.EqualTo(GameStatus.Ready), "Every level waits for the player.");
            Assert.That(_rig.Popup.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void LosingAndAcceptingTheEndingPlaysTheSameLevelAgain()
        {
            Play(GameRig.ValidLevel, Corridor4);
            PlayUntilTheLevelIsOver();
            Assert.That(_playthrough.Session.Status, Is.EqualTo(GameStatus.Lost));

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(0), "Losing is not progress.");
            Assert.That(BoardWidth, Is.EqualTo(16));
            Assert.That(_playthrough.Session.Game.Snake.Head, Is.EqualTo(new GridPosition(8, 8)));
            Assert.That(_playthrough.Session.Status, Is.EqualTo(GameStatus.Ready), "Every level waits for the player.");
        }

        [Test]
        public void TheSequenceLoopsBackToTheFirstLevelAfterTheLast()
        {
            Play(Corridor3, Corridor4);

            PlayUntilTheLevelIsOver();
            _rig.PopupButton.OnPointerClick(null);

            PlayUntilTheLevelIsOver();
            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_playthrough.LevelIndex, Is.EqualTo(0));
            Assert.That(BoardWidth, Is.EqualTo(3));
        }

        [Test]
        public void TheButtonOfAHiddenPopupChangesNothing()
        {
            Play(Corridor3, Corridor4);

            _rig.PopupButton.OnPointerClick(null);

            Assert.That(_rig.PopupButton.Interactable, Is.False);
            Assert.That(_playthrough.LevelIndex, Is.EqualTo(0));
            Assert.That(BoardWidth, Is.EqualTo(3), "A press that cannot reach the screen must not change the level.");
        }
    }
}
