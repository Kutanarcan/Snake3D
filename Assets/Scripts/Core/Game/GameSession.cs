using System;

namespace Game.Core
{
    public sealed class GameSession
    {
        private readonly SnakeGame _game;
        private readonly TickTimer _timer;
        private readonly IInputSource _input;
        private readonly Level _level;

        public GameSession(SnakeGame game, TickTimer timer, IInputSource input, Level level)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (timer == null)
            {
                throw new ArgumentNullException(nameof(timer));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            _game = game;
            _timer = timer;
            _input = input;
            _level = level;
        }

        public event Action Ate;
        public event Action Deflected;
        public event Action Turned;
        public event Action Won;
        public event Action Lost;

        public SnakeGame Game
        {
            get { return _game; }
        }

        public GameStatus Status { get; private set; }

        public int FoodEaten { get; private set; }

        public int FoodGoal
        {
            get { return _level.FoodGoal; }
        }

        public bool IsOver
        {
            get { return Status == GameStatus.Won || Status == GameStatus.Lost; }
        }

        public float NormalizedTick
        {
            get { return _timer.NormalizedTick; }
        }

        public void StartRun()
        {
            _game.Reset(_level.SnakeStart, _level.SnakeDirection, _level.SnakeLength);
            _timer.Reset();
            _input.Clear();

            FoodEaten = 0;
            Status = GameStatus.Ready;
        }

        public void Tick(float deltaTime)
        {
            if (IsOver)
            {
                return;
            }

            Direction direction;
            Direction heading = _game.Snake.PendingDirection;
            bool turned = _input.TryRead(out direction) && _game.TryTurn(direction);

            if (turned && direction != heading)
            {
                Raise(Turned);
            }

            if (Status == GameStatus.Ready && turned)
            {
                Status = GameStatus.Running;
            }

            if (Status != GameStatus.Running)
            {
                return;
            }

            _timer.Tick(deltaTime);

            while (Status == GameStatus.Running && _timer.TryConsumeTick())
            {
                Resolve(_game.Step());
            }
        }

        public void ClearListeners()
        {
            Ate = null;
            Deflected = null;
            Turned = null;
            Won = null;
            Lost = null;
        }

        private void Resolve(StepResult step)
        {
            if (step == StepResult.Died)
            {
                Finish(GameStatus.Lost, Lost);
                return;
            }

            if (_game.LastStepDeflected)
            {
                Raise(Deflected);
            }

            if (step != StepResult.Ate)
            {
                return;
            }

            FoodEaten += AppleSpawnManager.ValueOf(_game.LastMeal);

            bool shouldSpawnFood = !ReachedTheGoal
                && (_game.LastMeal != CellOccupantType.Food || _game.Food.TryRespawn());

            Raise(Ate);

            if (!shouldSpawnFood)
            {
                Finish(GameStatus.Won, Won);
            }
        }

        private bool ReachedTheGoal
        {
            get { return _level.FoodGoal > 0 && FoodEaten >= _level.FoodGoal; }
        }

        private void Finish(GameStatus status, Action listeners)
        {
            Status = status;
            Raise(listeners);
        }

        private static void Raise(Action listeners)
        {
            if (listeners != null)
            {
                listeners();
            }
        }
    }
}
