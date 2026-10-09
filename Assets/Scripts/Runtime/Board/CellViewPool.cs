using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class CellViewPool
    {
        private readonly CellView _prefab;
        private readonly Transform _idleRoot;
        private readonly Stack<CellView> _idle;

        public CellViewPool(CellView prefab, Transform idleRoot, int capacity)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (idleRoot == null)
            {
                throw new ArgumentNullException(nameof(idleRoot));
            }

            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _prefab = prefab;
            _idleRoot = idleRoot;
            _idle = new Stack<CellView>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                _idle.Push(Create());
            }
        }

        public int IdleCount
        {
            get { return _idle.Count; }
        }

        public CellView Rent(Transform parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            CellView view = _idle.Count > 0 ? _idle.Pop() : Create();

            view.Resize(1f);
            view.transform.SetParent(parent, false);
            view.gameObject.SetActive(true);

            return view;
        }

        public void Return(CellView view)
        {
            if (ReferenceEquals(view, null))
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
            _idle.Push(view);
        }

        private CellView Create()
        {
            CellView view = UnityEngine.Object.Instantiate(_prefab, _idleRoot, false);
            view.gameObject.SetActive(false);

            return view;
        }
    }
}
