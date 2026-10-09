using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class EffectPlayer : IEffectPlayer
    {
        private readonly EffectPool[] _pools;
        private readonly float _height;

        public EffectPlayer(EffectCatalog catalog, Transform root, float height)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _height = height;
            _pools = new EffectPool[catalog.Count];

            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new EffectPool(catalog.At(i), root);
            }
        }

        public void Play(EffectId id, GridPosition cell)
        {
            for (int i = 0; i < _pools.Length; i++)
            {
                if (_pools[i].Id == id)
                {
                    _pools[i].Play(new Vector3(cell.X, _height, cell.Y));
                    return;
                }
            }
        }

        public void StopAll()
        {
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i].StopAll();
            }
        }
    }
}
