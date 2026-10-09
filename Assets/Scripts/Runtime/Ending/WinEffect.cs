using System;
using UnityEngine;

namespace Game.Runtime
{
    [Serializable]
    public sealed class WinEffect
    {
        [SerializeField] private SoundId _winSound = SoundId.Win;
        [SerializeField, Min(0f)] private float _popupDelay = 0.5f;

        public SoundId WinSound { get { return _winSound; } }
        public float PopupDelay { get { return _popupDelay; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public WinEffect()
        {
        }

        public WinEffect(SoundId winSound, float popupDelay)
        {
            _winSound = winSound;
            _popupDelay = popupDelay;
        }
#endif
    }
}
