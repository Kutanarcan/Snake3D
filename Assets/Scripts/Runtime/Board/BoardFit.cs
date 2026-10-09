using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public readonly struct BoardFit
    {
        private BoardFit(float orthographicSize, float upShift, float depth, float centerX, float centerZ)
        {
            OrthographicSize = orthographicSize;
            UpShift = upShift;
            Depth = depth;
            CenterX = centerX;
            CenterZ = centerZ;
        }

        public float OrthographicSize { get; }
        public float UpShift { get; }
        public float Depth { get; }
        public float CenterX { get; }
        public float CenterZ { get; }

        public static BoardFit For(GridSize board, Quaternion rotation, float aspect,
            float padding, float topFraction, float bottomFraction)
        {
            float band = 1f - topFraction - bottomFraction;
            Validate(aspect, padding, band);

            float halfX = board.Width * 0.5f + padding;
            float halfZ = board.Height * 0.5f + padding;

            float alongUp = ExtentOn(rotation * Vector3.up, halfX, halfZ);
            float alongRight = ExtentOn(rotation * Vector3.right, halfX, halfZ);

            float size = Mathf.Max(alongUp / band, alongRight / aspect);
            float bandCentre = bottomFraction + band * 0.5f;

            return new BoardFit(
                size,
                (bandCentre - 0.5f) * 2f * size,
                ExtentOn(rotation * Vector3.forward, halfX, halfZ),
                (board.Width - 1) * 0.5f,
                (board.Height - 1) * 0.5f);
        }

        private static float ExtentOn(Vector3 axis, float halfX, float halfZ)
        {
            return Mathf.Abs(axis.x) * halfX + Mathf.Abs(axis.z) * halfZ;
        }

        private static void Validate(float aspect, float padding, float band)
        {
            if (aspect <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(aspect), "The viewport has no width or no height.");
            }

            if (padding < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(padding), "Padding cannot pull the board inwards.");
            }

            if (band <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(band), "The UI leaves no room on screen for the board.");
            }
        }
    }
}
