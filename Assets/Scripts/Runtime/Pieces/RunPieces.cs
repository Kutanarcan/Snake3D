using System;
using Game.Core;

namespace Game.Runtime
{
    public sealed class RunPieces
    {
        private readonly SnakeView _snake;
        private readonly AppleView _apple;
        private readonly AppleView _goldenApple;
        private readonly ISoundPlayer _sound;
        private readonly EffectPlayer _effects;
        private readonly Action _plainArrived;
        private readonly Action _goldenArrived;

        public RunPieces(SnakeView snake, AppleView apple, AppleView goldenApple, ISoundPlayer sound,
            EffectPlayer effects)
        {
            if (snake == null)
            {
                throw new ArgumentNullException(nameof(snake));
            }

            if (apple == null)
            {
                throw new ArgumentNullException(nameof(apple));
            }

            if (goldenApple == null)
            {
                throw new ArgumentNullException(nameof(goldenApple));
            }

            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }

            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }

            _snake = snake;
            _apple = apple;
            _goldenApple = goldenApple;
            _sound = sound;
            _effects = effects;
            _plainArrived = PlainArrived;
            _goldenArrived = GoldenArrived;
        }

        public void Show(Snake snake)
        {
            _snake.Show(snake);
        }

        public void Hide()
        {
            _effects.StopAll();
            _snake.Hide();
            _apple.Hide();
            _goldenApple.Hide();
        }

        public void Eaten(SnakeGame game)
        {
            _sound.Play(RunSounds.ForMeal(game.LastMeal));
            _effects.Play(RunEffects.ForMeal(game.LastMeal), game.Snake.Head);

            FollowApple(game.Apples.Plain);
        }

        public void FollowApple(FoodSpawner apple)
        {
            _apple.Follow(apple.IsOnTheBoard, apple.Position, _plainArrived);
        }

        public void Draw(SnakeGame game, float normalizedTick)
        {
            _snake.Render(normalizedTick);

            GoldenAppleSpawner golden = game.Apples.Golden;

            _goldenApple.Follow(golden.IsOnTheBoard, golden.Position, _goldenArrived);
        }

        private void PlainArrived()
        {
            _sound.Play(RunSounds.ForArrival(false));
        }

        private void GoldenArrived()
        {
            _sound.Play(RunSounds.ForArrival(true));
        }
    }
}
