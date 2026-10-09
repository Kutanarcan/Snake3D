using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class BoardCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(0f)] private float _padding = 0.5f;
        [SerializeField, Range(0f, 0.6f)] private float _topUiFraction = 0.1f;
        [SerializeField, Range(0f, 0.6f)] private float _bottomUiFraction = 0.25f;

        [SerializeField, Min(0f)] private float _clearance = 10f;

        private GridSize _board;
        private bool _framed;
        private float _aspect;

        public float Yaw
        {
            get { return _camera.transform.eulerAngles.y; }
        }

        public void Frame(GridSize board)
        {
            _board = board;
            _framed = true;

            Fit();
        }

        private void LateUpdate()
        {
            if (!_framed || Mathf.Approximately(_camera.aspect, _aspect))
            {
                return;
            }

            Fit();
        }

        private void Fit()
        {
            Transform view = _camera.transform;
            _aspect = _camera.aspect;

            BoardFit fit = BoardFit.For(
                _board, view.rotation, _aspect, _padding, _topUiFraction, _bottomUiFraction);

            _camera.orthographic = true;
            _camera.orthographicSize = fit.OrthographicSize;

            Vector3 centre = new Vector3(fit.CenterX, 0f, fit.CenterZ);

            view.position = centre
                - view.forward * (fit.Depth + _camera.nearClipPlane + _clearance)
                - view.up * fit.UpShift;
        }

        public Result Validate()
        {
            if (_camera == null)
            {
                return Result.Failure("BoardCamera has no camera assigned.");
            }

            if (_topUiFraction + _bottomUiFraction >= 1f)
            {
                return Result.Failure("BoardCamera leaves no room for the board: "
                    + _topUiFraction + " of the screen on top plus " + _bottomUiFraction + " at the bottom.");
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(Camera camera, float topUiFraction, float bottomUiFraction)
        {
            _camera = camera;
            _topUiFraction = topUiFraction;
            _bottomUiFraction = bottomUiFraction;
        }
#endif
    }
}
