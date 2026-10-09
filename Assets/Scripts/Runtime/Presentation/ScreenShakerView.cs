using DG.Tweening;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class ScreenShakerView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _duration = 0.35f;
        [SerializeField, Min(0f)] private float _strength = 0.6f;
        [SerializeField, Min(0)] private int _vibrato = 18;
        [SerializeField, Range(0f, 180f)] private float _randomness = 90f;

        private Tween _shake;

        public void Shake()
        {
            Stop();

            if (_duration <= 0f || _strength <= 0f)
            {
                return;
            }

            _shake = _target
                .DOShakePosition(_duration, _strength, _vibrato, _randomness, false, true)
                .SetUpdate(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void Stop()
        {
            if (_shake == null)
            {
                return;
            }

            _shake.Kill(true);
            _shake = null;
        }

        public Result Validate()
        {
            return _target == null
                ? Result.Failure("ScreenShakerView has no target assigned.")
                : Result.Success();
        }

        private void OnDestroy()
        {
            Stop();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(Transform target)
        {
            _target = target;
        }
#endif
    }
}
