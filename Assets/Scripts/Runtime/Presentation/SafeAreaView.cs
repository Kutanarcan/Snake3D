using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class SafeAreaView : MonoBehaviour
    {
        [SerializeField] private RectTransform _area;

        private Rect _safeArea;
        private int _width;
        private int _height;

        public Result Validate()
        {
            if (_area == null)
            {
                return Result.Failure("SafeAreaView has no area assigned.");
            }

            return Result.Success();
        }

        private void OnEnable()
        {
            if (_area == null)
            {
                enabled = false;
                return;
            }

            Apply();
        }

        private void Update()
        {
            if (Screen.width == _width && Screen.height == _height && Screen.safeArea == _safeArea)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            _width = Screen.width;
            _height = Screen.height;
            _safeArea = Screen.safeArea;

            SafeAreaFit fit = SafeAreaFit.For(_safeArea, _width, _height);

            _area.anchorMin = fit.AnchorMin;
            _area.anchorMax = fit.AnchorMax;
            _area.offsetMin = Vector2.zero;
            _area.offsetMax = Vector2.zero;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(RectTransform area)
        {
            _area = area;
        }
#endif
    }
}
