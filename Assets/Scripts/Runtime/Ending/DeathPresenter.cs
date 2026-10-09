using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class DeathPresenter
    {
        private readonly ISoundPlayer _sound;
        private readonly ScreenShakerView _shaker;
        private readonly GameObject _link;
        private readonly TweenCallback _onFinished;

        private Tween _wait;
        private DeathEffect _effect;
        private Action _pendingComplete;

        public DeathPresenter(ISoundPlayer sound, ScreenShakerView shaker, GameObject link)
        {
            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }

            if (shaker == null)
            {
                throw new ArgumentNullException(nameof(shaker));
            }

            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            _sound = sound;
            _shaker = shaker;
            _link = link;

            _onFinished = HandleFinished;
        }

        public void Play(DeathEffect effect, Action onComplete)
        {
            Kill();

            _effect = effect;
            _pendingComplete = onComplete;

            _sound.Play(effect.HitSound);

            if (effect.ShakeScreen)
            {
                _shaker.Shake();
            }

            if (effect.LoseDelay <= 0f)
            {
                HandleFinished();

                return;
            }

            _wait = DOVirtual.DelayedCall(effect.LoseDelay, _onFinished, true)
                .SetUpdate(true)
                .SetLink(_link, LinkBehaviour.KillOnDestroy);
        }

        public void Kill()
        {
            _shaker.Stop();

            if (_wait != null)
            {
                _wait.Kill();
                _wait = null;
            }

            _effect = null;
            _pendingComplete = null;
        }

        private void HandleFinished()
        {
            _wait = null;
            _sound.Play(_effect.LoseSound);
            _effect = null;

            Action callback = _pendingComplete;
            _pendingComplete = null;

            if (callback != null)
            {
                callback();
            }
        }
    }
}
