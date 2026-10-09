using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class BoardView
    {
        private readonly CellViewPool _pool;
        private readonly Transform _root;
        private readonly BoardVisuals _visuals;
        private readonly List<CellView> _cells = new List<CellView>();

        public BoardView(CellViewPool pool, Transform root, BoardVisuals visuals)
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

        public void Show(GridSize board)
        {
            Hide();

            for (int z = 0; z < board.Height; z++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    CellView cell = _pool.Rent(_root);

                    cell.SetMesh(((x + z) & 1) == 0 ? _visuals.BoardDark : _visuals.BoardLight);
                    cell.Place(x, _visuals.BoardHeight, z);
                    _cells.Add(cell);
                }
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
