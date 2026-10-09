using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class GroundView
    {
        private readonly CellViewPool _pool;
        private readonly Transform _root;
        private readonly BoardVisuals _visuals;
        private readonly List<CellView> _cells = new List<CellView>();

        public GroundView(CellViewPool pool, Transform root, BoardVisuals visuals)
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

        public void Show(Level level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            Hide();

            GridSize board = level.Board;

            for (int index = 0; index < board.CellCount; index++)
            {
                GridPosition position = board.FromIndex(index);
                CellGroundType type = level.GroundAt(position);

                if (type == CellGroundType.Open)
                {
                    continue;
                }

                GroundVisual visual = _visuals.GroundFor(type);
                CellView cell = _pool.Rent(_root);

                cell.SetMesh(visual.Mesh);
                cell.Place(position.X, visual.Height, position.Y);
                _cells.Add(cell);
            }
        }

        public void Hide()
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                _pool.Return(_cells[i]);
            }

            _cells.Clear();
        }
    }
}
