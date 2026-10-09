using System;
using DG.Tweening;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class AppleView
    {
        private readonly CellViewPool _pool;
        private readonly Transform _root;
        private readonly Mesh _mesh;
        private readonly float _height;
        private readonly float _scale;
        private readonly float _appearDuration;
        private readonly TweenCallback _onAppeared;

        private CellView _apple;
        private GridPosition _at;
        private Action _arrived;

        public AppleView(CellViewPool pool, Transform root, Mesh mesh, float height, float scale,
            float appearDuration)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            _pool = pool;
            _root = root;
            _mesh = mesh;
            _height = height;
            _scale = scale;
            _appearDuration = appearDuration;
            _onAppeared = HandleAppeared;
        }

        public Transform Root
        {
            get { return _root; }
        }

        public void Follow(bool isOnTheBoard, GridPosition position, Action arrived)
        {
            if (!isOnTheBoard)
            {
                Hide();

                return;
            }

            Show(position, arrived);
        }

        public void Hide()
        {
            if (_apple == null)
            {
                return;
            }

            _apple.transform.DOKill();
            _arrived = null;

            _pool.Return(_apple);
            _apple = null;
        }

        private void Show(GridPosition position, Action arrived)
        {
            bool arriving = _apple == null || _at != position;

            if (_apple == null)
            {
                _apple = _pool.Rent(_root);
                _apple.SetMesh(_mesh);
            }

            _at = position;
            _apple.Place(position.X, _height, position.Y);

            if (!arriving)
            {
                return;
            }

            _apple.transform.DOKill();
            _arrived = arrived;

            if (_appearDuration <= 0f)
            {
                _apple.Resize(_scale);
                HandleAppeared();

                return;
            }

            _apple.Resize(0f);

            _apple.transform
                .DOScale(Vector3.one * _scale, _appearDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(_apple.gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(_onAppeared);
        }

        private void HandleAppeared()
        {
            Action callback = _arrived;
            _arrived = null;

            if (callback != null)
            {
                callback();
            }
        }
    }
}
