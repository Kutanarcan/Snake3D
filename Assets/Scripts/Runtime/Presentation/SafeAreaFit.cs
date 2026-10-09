using UnityEngine;

namespace Game.Runtime
{
    public readonly struct SafeAreaFit
    {
        private SafeAreaFit(Vector2 anchorMin, Vector2 anchorMax)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
        }

        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }

        public static SafeAreaFit For(Rect safeArea, int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return Whole;
            }

            Vector2 min = Clamped(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            Vector2 max = Clamped(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);

            if (max.x <= min.x || max.y <= min.y)
            {
                return Whole;
            }

            return new SafeAreaFit(min, max);
        }

        private static SafeAreaFit Whole
        {
            get { return new SafeAreaFit(Vector2.zero, Vector2.one); }
        }

        private static Vector2 Clamped(float x, float y)
        {
            return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
        }
    }
}
