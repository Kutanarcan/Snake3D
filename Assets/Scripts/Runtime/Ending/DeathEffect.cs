using System;
using UnityEngine;

namespace Game.Runtime
{
    [Serializable]
    public sealed class DeathEffect
    {
        [SerializeField] private EndingId _id;

        [Header("Hit")]
        [SerializeField] private SoundId _hitSound = SoundId.WallHit;

        [Header("Lose")]
        [SerializeField, Min(0f)] private float _loseDelay = 1f;
        [SerializeField] private SoundId _loseSound = SoundId.Lose;

        [Header("Feel")]
        [SerializeField] private bool _shakeScreen;

        public EndingId Id { get { return _id; } }
        public SoundId HitSound { get { return _hitSound; } }
        public float LoseDelay { get { return _loseDelay; } }
        public SoundId LoseSound { get { return _loseSound; } }
        public bool ShakeScreen { get { return _shakeScreen; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public DeathEffect()
        {
        }

        public DeathEffect(EndingId id, SoundId hitSound, float loseDelay, SoundId loseSound)
        {
            _id = id;
            _hitSound = hitSound;
            _loseDelay = loseDelay;
            _loseSound = loseSound;
        }

        public DeathEffect(EndingId id, SoundId hitSound, float loseDelay, SoundId loseSound, bool shakeScreen)
            : this(id, hitSound, loseDelay, loseSound)
        {
            _shakeScreen = shakeScreen;
        }
#endif
    }
}
