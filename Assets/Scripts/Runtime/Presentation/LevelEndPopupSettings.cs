using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(fileName = "LevelEndPopupSettings", menuName = "Snake/Level End Popup Settings")]
    public sealed class LevelEndPopupSettings : ScriptableObject
    {
        [SerializeField] private LevelEndingInformation _won;
        [SerializeField] private LevelEndingInformation _lost;
        [SerializeField, Range(0f, 1f)] private float _dimAlpha = 0.6f;
        [SerializeField, Min(0f)] private float _duration = 0.25f;
        [SerializeField, Min(0f)] private float _fromScale = 0.8f;

        public float DimAlpha { get { return _dimAlpha; } }
        public float Duration { get { return _duration; } }
        public float FromScale { get { return _fromScale; } }

        public LevelEndingInformation GetLevelEndingInformation(GameStatus status)
        {
            return status == GameStatus.Won ? _won : _lost;
        }

        public Result Validate()
        {
            Result won = _won.Validate("won");
            if (!won.IsSuccess)
            {
                return won;
            }

            return _lost.Validate("lost");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(LevelEndingInformation won, LevelEndingInformation lost)
        {
            _won = won;
            _lost = lost;
        }
#endif
    }
}
