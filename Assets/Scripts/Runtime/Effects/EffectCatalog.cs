using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(fileName = "EffectCatalog", menuName = "Snake/Effect Catalog")]
    public sealed class EffectCatalog : ScriptableObject
    {
        [SerializeField] private EffectEntry[] _effects;

        public int Count
        {
            get { return _effects == null ? 0 : _effects.Length; }
        }

        public EffectEntry At(int index)
        {
            return _effects[index];
        }

        public Result Validate()
        {
            for (int i = 0; i < Count; i++)
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
            EffectEntry entry = _effects[index];

            if (entry == null)
            {
                return Result.Failure("EffectCatalog has an empty row at slot " + index + ".");
            }

            if (entry.Id == EffectId.None)
            {
                return Result.Failure("EffectCatalog has a row at slot " + index + " that names no effect.");
            }

            if (entry.Prefab == null || entry.Prefab.Particles == null)
            {
                return Result.Failure("EffectCatalog has no particle prefab for " + entry.Id + ".");
            }

            for (int i = 0; i < index; i++)
            {
                if (_effects[i] != null && _effects[i].Id == entry.Id)
                {
                    return Result.Failure("EffectCatalog has two rows for " + entry.Id + ".");
                }
            }

            return ValidatePrefab(entry);
        }

        private static Result ValidatePrefab(EffectEntry entry)
        {
            ParticleSystem.MainModule main = entry.Prefab.Particles.main;

            if (main.loop)
            {
                return Result.Failure("The particle prefab for " + entry.Id + " loops, so it never goes back to the pool.");
            }

            if (main.stopAction != ParticleSystemStopAction.Callback)
            {
                return Result.Failure("The particle prefab for " + entry.Id + " needs its Stop Action set to Callback.");
            }

            if (main.playOnAwake)
            {
                return Result.Failure("The particle prefab for " + entry.Id + " plays on awake, so the pool would fire it while filling.");
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(EffectEntry[] effects)
        {
            _effects = effects;
        }
#endif
    }
}
