using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class SnakeView
    {
        private static readonly Quaternion[] Facings =
        {
            Quaternion.Euler(0f, 0f, 0f),
            Quaternion.Euler(0f, 90f, 0f),
            Quaternion.Euler(0f, 180f, 0f),
            Quaternion.Euler(0f, 270f, 0f)
        };

        private readonly CellViewPool _pool;
        private readonly Transform _root;
        private readonly BoardVisuals _visuals;
        private readonly List<CellView> _parts = new List<CellView>();

        private SnakeTrail _trail;

        public SnakeView(CellViewPool pool, Transform root, BoardVisuals visuals)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (visuals == null)
            {
                throw new ArgumentNullException(nameof(visuals));
            }

            _pool = pool;
            _root = root;
            _visuals = visuals;
        }

        public Transform Root
        {
            get { return _root; }
        }

        public void Show(Snake snake)
        {
            Hide();

            _trail = new SnakeTrail(snake);
            _trail.Restart();

            Fit();
        }

        public void Hide()
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                _pool.Return(_parts[i]);
            }

            _parts.Clear();
        }

        public void Render(float normalizedTick)
        {
            _trail.Sync();
            Fit();

            float step = StepEase.Apply(normalizedTick, _visuals.StepEase);

            for (int i = 0; i < _parts.Count; i++)
            {
                GridPosition from = _trail.From(i);
                GridPosition to = _trail.To(i);
                CellView part = _parts[i];

                part.Place(
                    Mathf.Lerp(from.X, to.X, step),
                    _visuals.PieceHeight,
                    Mathf.Lerp(from.Y, to.Y, step));

                part.Face(Facings[(int)_trail.Facing(i)]);
            }
        }

        private void Fit()
        {
            while (_parts.Count < _trail.Length)
            {
                Grow();
            }
        }

        private void Grow()
        {
            _parts.Add(_pool.Rent(_root));

            int last = _parts.Count - 1;
            if (last > 0)
            {
                _parts[last - 1].SetMesh(GetMesh(last - 1));
            }

            _parts[last].SetMesh(GetMesh(last));
        }

        private Mesh GetMesh(int index)
        {
            if (index == 0)
            {
                return _visuals.SnakeHead;
            }

            return index == _parts.Count - 1 ? _visuals.SnakeTail : _visuals.SnakeBody;
        }
    }
}
