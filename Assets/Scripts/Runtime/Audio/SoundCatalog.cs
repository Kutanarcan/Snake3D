using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(fileName = "SoundCatalog", menuName = "Snake/Sound Catalog")]
    public sealed class SoundCatalog : ScriptableObject
    {
        [SerializeField] private SoundEntry[] _sounds;

        public SoundEntry Find(SoundId id)
        {
            if (_sounds == null || id == SoundId.None)
            {
                return null;
            }

            for (int i = 0; i < _sounds.Length; i++)
            {
                if (_sounds[i] != null && _sounds[i].Id == id)
                {
                    return _sounds[i];
                }
            }

            return null;
        }

        public Result Validate()
        {
            if (_sounds == null)
            {
                return Result.Success();
            }

            for (int i = 0; i < _sounds.Length; i++)
            {
                Result row = ValidateRow(i);
                if (!row.IsSuccess)
                {
                    return row;
                }
            }

            return Result.Success();
        }

        private Result ValidateRow(int index)
        {
            SoundEntry entry = _sounds[index];

            if (entry == null)
            {
                return Result.Failure("SoundCatalog has an empty row at slot " + index + ".");
            }

            if (entry.Id == SoundId.None)
            {
                return Result.Failure("SoundCatalog has a row at slot " + index + " that names no sound.");
            }

            if (entry.Clip == null)
            {
                return Result.Failure("SoundCatalog has no clip for " + entry.Id + ".");
            }

            for (int i = 0; i < index; i++)
            {
                if (_sounds[i] != null && _sounds[i].Id == entry.Id)
                {
                    return Result.Failure("SoundCatalog has two rows for " + entry.Id + ".");
                }
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(SoundEntry[] sounds)
        {
            _sounds = sounds;
        }
#endif
    }
}
