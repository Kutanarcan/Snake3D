using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class WinPresenter
    {
        private readonly ISoundPlayer _sound;
        private readonly GameObject _link;
        private readonly TweenCallback _onFinished;

        private Tween _wait;
        private Action _pendingComplete;

        public WinPresenter(ISoundPlayer sound, GameObject link)
        {
            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }

            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            _sound = sound;
            _link = link;
            _onFinished = HandleFinished;
        }

        public void Play(WinEffect effect, Action onComplete)
        {
            Kill();

            _pendingComplete = onComplete;
            _sound.Play(effect.WinSound);

            if (effect.PopupDelay <= 0f)
            {
                HandleFinished();

                return;
            }

            _wait = DOVirtual.DelayedCall(effect.PopupDelay, _onFinished, true)
                .SetUpdate(true)
                .SetLink(_link, LinkBehaviour.KillOnDestroy);
        }

        public void Kill()
        {
            if (_wait != null)
            {
                _wait.Kill();
                _wait = null;
            }

            _pendingComplete = null;
        }

        private void HandleFinished()
        {
            _wait = null;

            Action callback = _pendingComplete;
            _pendingComplete = null;

            if (callback != null)
            {
                callback();
            }
        }
    }
}
