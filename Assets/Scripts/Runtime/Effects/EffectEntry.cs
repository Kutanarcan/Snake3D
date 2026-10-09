using System;
using UnityEngine;

namespace Game.Runtime
{
    [Serializable]
    public sealed class EffectEntry
    {
        [SerializeField] private EffectId _id;
        [SerializeField] private EffectView _prefab;
        [SerializeField, Min(0)] private int _prewarm = 4;
        [SerializeField, Min(0.01f)] private float _scale = 1f;

        public EffectId Id { get { return _id; } }
        public EffectView Prefab { get { return _prefab; } }
        public int Prewarm { get { return _prewarm; } }
        public float Scale { get { return _scale; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public EffectEntry()
        {
        }

        public EffectEntry(EffectId id, EffectView prefab, int prewarm, float scale)
        {
            _id = id;
            _prefab = prefab;
            _prewarm = prewarm;
            _scale = scale;
        }
#endif
    }
}
