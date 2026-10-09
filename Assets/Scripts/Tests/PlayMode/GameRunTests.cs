using System;
using System.Collections.Generic;
using Game.Core;
using Game.Input;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.PlayMode
{
    public sealed class GameRunTests
    {
        // The board the food lands on is decided here rather than by chance, so
        // a test can put an apple in front of the snake and know it gets eaten.
        private sealed class PinnedRandomSource : IRandomSource
        {
            public GridMap Map;
            public GridPosition Target;

            public int Next(int maxExclusive)
            {
                for (int index = 0; index < maxExclusive; index++)
                {
                    if (Map.FreeCellAt(index) == Target)
                    {
                        return index;
                    }
                }

                return 0;
            }

            // The target is fixed by the test, so a seed has nothing to change here.
            public void Reseed(int seed)
            {
            }
        }

        // Writes down what it was asked to play instead of playing it, so a test
        // can read the run's soundtrack back without a speaker in sight.
        private sealed class RecordingSoundPlayer : ISoundPlayer
        {
            public readonly List<SoundId> Played = new List<SoundId>();

            public void Play(SoundId id)
            {
                Played.Add(id);
            }

            public int CountOf(SoundId id)
            {
                int count = 0;

                for (int i = 0; i < Played.Count; i++)
                {
                    if (Played[i] == id)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private GameRig _rig;
        private RecordingSoundPlayer _sounds;

        private EffectPlayer NewEffects()
        {
            return new EffectPlayer(_rig.Effects, _rig.NewRoot("effects"), 1f);
        }

        private RunTimeline _timeline;
        private Player_Input _actions;
        private PinnedRandomSource _random;
        private ArrowInput _arrows;
        private GameSession _session;
        private GameRun _run;

        // The run is built here rather than taken from the installer, because
        // what is under test is the run itself and not the scene it lives in.
        [SetUp]
        public void SetUp()
        {
            _rig = new GameRig(GameRig.ValidLevel);

            // One cell per square of the rig's board, so a run that grows into
            // every square still draws from stock.
            CellViewPool pool = new CellViewPool(_rig.CellPrefab, _rig.Host.transform, 16 * 16);
            SnakeView snake = new SnakeView(pool, _rig.NewRoot("snake"), _rig.Visuals);
            AppleView food = _rig.NewApple(pool, "food");
            AppleView golden = _rig.NewGoldenApple(pool, "golden");

            _sounds = new RecordingSoundPlayer();
            _timeline = new RunTimeline(_rig.Endings, _sounds, _rig.Shaker, _rig.Host);
            _actions = new Player_Input();
            _run = new GameRun(_actions, new RunPieces(snake, food, golden, _sounds, NewEffects()), _rig.Information,
                _rig.Popup, _sounds, _timeline);

            Build(GameRig.ValidLevel);
        }

        private void Build(string json)
        {
            Level level;
            Result loadResult = LevelLoader.TryLoad(json, out level);
            Assert.That(loadResult.IsSuccess, Is.True, loadResult.Error);

            _random = new PinnedRandomSource();
            _arrows = new ArrowInput();
            _session = LevelBuilder.Build(level, GameRig.Interval, _random, _arrows);

            // The corner the snake never walks through, so the food stays out of
            // the way unless a test pins it somewhere on purpose.
            _random.Map = _session.Game.Map;
            _random.Target = new GridPosition(0, 0);
        }

        [TearDown]
        public void TearDown()
        {
            _actions.Dispose();
            _rig.Destroy();
        }

        // A run waits for the player before it moves, so a test that wants steps
        // has to ask for the first one. The level already faces Right, so this
        // sets the run off and steers nothing. Begin clears whatever was pressed
        // before it, so this only counts afterwards.
        private void SetOff()
        {
            _arrows.Press(Direction.Right);
        }

        // The level starts the snake at (8,8) heading right on a board 16 wide,
        // so it walks off the edge on the eighth step. Nothing can end the run
        // before that: ten apples in eight steps is not a run that exists.
        private void WalkUntilTheRunIsOver()
        {
            SetOff();

            for (int frame = 0; frame < 64 && !_run.Session.IsOver; frame++)
            {
                _run.Tick(GameRig.Interval);
            }

            Assert.That(_run.Session.IsOver, Is.True, "The snake never reached the wall.");
        }

        // What the label pads itself with is a layout decision, so the tests read
        // the value and leave the spacing to the view.
        private string Counter
        {
            get { return _rig.FoodAmount.text.Trim(); }
        }

        [Test]
        public void ARunNeedsASessionToBegin()
        {
            Assert.Throws<ArgumentNullException>(() => _run.Begin(null));
        }

        [Test]
        public void TheCounterStartsAtNothingEatenOutOfTheGoal()
        {
            _run.Begin(_session);

            Assert.That(Counter, Is.EqualTo("0/10"));
        }

        [Test]
        public void TheCounterFollowsWhatTheRunEats()
        {
            // (9,8) is the cell the snake steps onto first, so the very next
            // tick is a bite.
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();
            _random.Target = new GridPosition(0, 0);

            _run.Tick(GameRig.Interval);

            Assert.That(_run.Session.FoodEaten, Is.EqualTo(1), "The bite never happened.");
            Assert.That(Counter, Is.EqualTo("1/10"));
        }

        [Test]
        public void TheCounterGoesBackToNothingWhenTheRunStartsAgain()
        {
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();
            _random.Target = new GridPosition(0, 0);
            _run.Tick(GameRig.Interval);

            _run.End();
            _run.Begin(_session);

            Assert.That(Counter, Is.EqualTo("0/10"));
        }

        [Test]
        public void TheRunPutsThePopupOnScreenSayingHowItEnded()
        {
            _run.Begin(_session);
            Assert.That(_rig.Popup.gameObject.activeSelf, Is.False, "A running game shows no popup.");

            WalkUntilTheRunIsOver();

            Assert.That(_rig.Popup.gameObject.activeSelf, Is.True);
            Assert.That(_rig.PopupTitle.text, Is.EqualTo(GameRig.LostTitle));
            Assert.That(_rig.PopupStar.sprite, Is.SameAs(_rig.LostStar), "The losing star is the one on screen.");
            Assert.That(_rig.PopupButtonLabel.text, Is.EqualTo(GameRig.LostButton));
        }

        [Test]
        public void AWonRunWearsTheOtherStarAndSendsThePlayerOnward()
        {
            // A goal of one apple, put where the snake steps first, so the very
            // next tick wins the level.
            Build(GameRig.ValidLevel.Replace(@"""FoodGoal"": 10", @"""FoodGoal"": 1"));
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();

            _run.Tick(GameRig.Interval);

            Assert.That(_run.Session.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(_rig.PopupTitle.text, Is.EqualTo(GameRig.WonTitle));
            Assert.That(_rig.PopupStar.sprite, Is.SameAs(_rig.WonStar));
            Assert.That(_rig.PopupButtonLabel.text, Is.EqualTo(GameRig.WonButton));
        }

        [Test]
        public void StartingAgainTakesThePopupOffTheScreen()
        {
            _run.Begin(_session);
            WalkUntilTheRunIsOver();

            _run.End();
            _run.Begin(_session);

            Assert.That(_rig.Popup.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void TheRunStopsReadingTheDeviceOnceItIsOver()
        {
            _run.Begin(_session);
            Assert.That(_actions.Player.enabled, Is.True, "A running game reads the device.");

            WalkUntilTheRunIsOver();

            Assert.That(_actions.Player.enabled, Is.False);
        }

        [Test]
        public void AnEndedRunShowsNoPopupOfItsOwn()
        {
            _run.Begin(_session);

            _run.End();
            WalkUntilTheRunIsOver();

            Assert.That(_rig.Popup.gameObject.activeSelf, Is.False,
                "A run that was ended must not put its own popup on screen.");
        }

        [Test]
        public void TheBiteIsHeard()
        {
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();
            _random.Target = new GridPosition(0, 0);

            _run.Tick(GameRig.Interval);

            Assert.That(_sounds.CountOf(SoundId.AppleEat), Is.EqualTo(1));
        }

        [Test]
        public void TheAppleAnnouncesItselfWhenTheLevelLaysItOut()
        {
            _run.Begin(_session);

            Assert.That(_sounds.CountOf(SoundId.AppleAppear), Is.EqualTo(1));
        }

        [Test]
        public void AnAppleThatMovedToANewCellAnnouncesItselfAgain()
        {
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();
            _random.Target = new GridPosition(0, 0);

            _run.Tick(GameRig.Interval);

            Assert.That(_sounds.CountOf(SoundId.AppleAppear), Is.EqualTo(2),
                "The one the level laid out, and the one the bite moved somewhere new.");
        }

        [Test]
        public void LosingIsHeard()
        {
            _run.Begin(_session);

            WalkUntilTheRunIsOver();

            Assert.That(_sounds.CountOf(SoundId.Lose), Is.EqualTo(1));
            Assert.That(_sounds.CountOf(SoundId.Win), Is.EqualTo(0));
        }

        [Test]
        public void WinningIsHeard()
        {
            Build(GameRig.ValidLevel.Replace(@"""FoodGoal"": 10", @"""FoodGoal"": 1"));
            _random.Target = new GridPosition(9, 8);
            _run.Begin(_session);
            SetOff();

            _run.Tick(GameRig.Interval);

            Assert.That(_sounds.CountOf(SoundId.Win), Is.EqualTo(1));
            Assert.That(_sounds.CountOf(SoundId.Lose), Is.EqualTo(0));
        }
    }
}
