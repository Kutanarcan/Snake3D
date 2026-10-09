using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class AudioManager : MonoBehaviour, ISoundPlayer
    {
        [SerializeField] private SoundCatalog _catalog;
        [SerializeField] private AudioSource _source;

        public void Play(SoundId id)
        {
            SoundEntry sound = _catalog.Find(id);

            if (sound == null || sound.Clip == null)
            {
                return;
            }

            _source.PlayOneShot(sound.Clip, sound.Volume);
        }

        public Result Validate()
        {
            if (_catalog == null)
            {
                return Missing("catalog");
            }

            if (_source == null)
            {
                return Missing("audio source");
            }

            return _catalog.Validate();
        }

        private static Result Missing(string field)
        {
            return Result.Failure("AudioManager has no " + field + " assigned.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(SoundCatalog catalog, AudioSource source)
        {
            _catalog = catalog;
            _source = source;
        }
#endif
    }
}
