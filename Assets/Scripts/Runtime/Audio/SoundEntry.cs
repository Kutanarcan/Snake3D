using System;
using UnityEngine;

namespace Game.Runtime
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private SoundId _id;
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        public SoundId Id { get { return _id; } }
        public AudioClip Clip { get { return _clip; } }
        public float Volume { get { return _volume; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public SoundEntry()
        {
        }

        public SoundEntry(SoundId id, AudioClip clip, float volume)
        {
            _id = id;
            _clip = clip;
            _volume = volume;
        }
#endif
    }
}
