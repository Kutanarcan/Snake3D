using System;
using Game.Core;

namespace Game.Runtime
{
    public sealed class Playthrough
    {
        private readonly LevelSequence _levels;
        private readonly GameRun _run;
        private readonly BoardPresentation _board;
        private readonly LevelEndPopupView _popup;
        private readonly float _tickInterval;
        private readonly IRandomSource _random;
        private readonly IInputSource _input;
        private readonly ILevelPersistent _levelPersistent;
        private readonly ISoundPlayer _sound;

        public Playthrough(LevelSequence levels, GameRun run, BoardPresentation board, LevelEndPopupView popup,
            float tickInterval, IRandomSource random, IInputSource input, ILevelPersistent levelPersistent,
            ISoundPlayer sound)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (popup == null)
            {
                throw new ArgumentNullException(nameof(popup));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (levelPersistent == null)
            {
                throw new ArgumentNullException(nameof(levelPersistent));
            }

            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }

            _levels = levels;
            _run = run;
            _board = board;
            _popup = popup;
            _tickInterval = tickInterval;
            _random = random;
            _input = input;
            _levelPersistent = levelPersistent;
            _sound = sound;
        }

        public GameSession Session
        {
            get { return _run.Session; }
        }

        public int LevelIndex
        {
            get { return _levels.Index; }
        }

        public void Begin()
        {
            _popup.Initialize(OnEndingButtonPressed);
            _levels.GoTo(_levelPersistent.Read());

            SetUpTheCurrentLevel();
        }

        public void Tick(float deltaTime)
        {
            _run.Tick(deltaTime);
        }

        private void OnEndingButtonPressed()
        {
            _sound.Play(SoundId.ButtonClick);

            if (_run.Session.Status == GameStatus.Won)
            {
                _levels.Advance();
                _levelPersistent.Write(_levels.Index);
            }

            _run.End();

            SetUpTheCurrentLevel();
        }

        private void SetUpTheCurrentLevel()
        {
            Level level = _levels.Current;

            _board.Show(level);

            _run.Begin(LevelBuilder.Build(level, _tickInterval, _random, _input));
        }
    }
}
