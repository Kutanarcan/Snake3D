using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class RunTimeline
    {
        private readonly EndingSettings _settings;
        private readonly DeathPresenter _death;
        private readonly WinPresenter _win;

        public RunTimeline(EndingSettings settings, ISoundPlayer sound, ScreenShakerView shaker, GameObject link)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings;
            _death = new DeathPresenter(sound, shaker, link);
            _win = new WinPresenter(sound, link);
        }

        public void Play(GameStatus status, SnakeGame game, Action onComplete)
        {
            if (onComplete == null)
            {
                throw new ArgumentNullException(nameof(onComplete));
            }

            Kill();

            if (status == GameStatus.Won)
            {
                _win.Play(_settings.Win, onComplete);
                return;
            }

            DeathEffect death = _settings.Find(RunEndings.For(game));

            if (death == null)
            {
                onComplete();
                return;
            }

            _death.Play(death, onComplete);
        }

        public void Kill()
        {
            _death.Kill();
            _win.Kill();
        }
    }
}
