using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class EffectPool
    {
        private readonly EffectEntry _entry;
        private readonly Transform _root;
        private readonly Stack<EffectView> _idle;
        private readonly List<EffectView> _live;
        private readonly Action<EffectView> _finished;

        public EffectPool(EffectEntry entry, Transform root)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _entry = entry;
            _root = root;
            _idle = new Stack<EffectView>(entry.Prewarm);
            _live = new List<EffectView>(entry.Prewarm);
            _finished = Return;

            for (int i = 0; i < entry.Prewarm; i++)
            {
                _idle.Push(Create());
            }
        }

        public EffectId Id
        {
            get { return _entry.Id; }
        }

        public int IdleCount
        {
            get { return _idle.Count; }
        }

        public int LiveCount
        {
            get { return _live.Count; }
        }

        public void Play(Vector3 position)
        {
            EffectView view = _idle.Count > 0 ? _idle.Pop() : Create();

            _live.Add(view);
            view.gameObject.SetActive(true);
            view.Play(position, _entry.Scale);
        }

        public void StopAll()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                EffectView view = _live[i];

                view.Stop();
                Return(view);
            }
        }

        private void Return(EffectView view)
        {
            if (!_live.Remove(view))
            {
                return;
            }

            view.gameObject.SetActive(false);
            _idle.Push(view);
        }

        private EffectView Create()
        {
            EffectView view = UnityEngine.Object.Instantiate(_entry.Prefab, _root, false);
            view.Initialize(_finished);
            view.gameObject.SetActive(false);

            return view;
        }
    }
}
