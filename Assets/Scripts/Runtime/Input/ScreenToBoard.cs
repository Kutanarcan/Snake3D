using System;

namespace Game.Runtime
{
    public sealed class ScreenToBoard
    {
        private const double HalfStep = 45.0;
        private const double Radians = Math.PI / 180.0;

        private readonly float _cos;
        private readonly float _sin;

        public ScreenToBoard(float cameraYaw)
        {
            double turn = (cameraYaw - HalfStep) * Radians;

            _cos = (float)Math.Cos(turn);
            _sin = (float)Math.Sin(turn);
        }

        public void Map(float screenX, float screenY, out float boardX, out float boardY)
        {
            boardX = screenX * _cos + screenY * _sin;
            boardY = screenY * _cos - screenX * _sin;
        }
    }
}
