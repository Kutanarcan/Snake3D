using System;

namespace Game.Runtime
{
    public sealed class FrameSampler
    {
        private readonly float _interval;

        private float _elapsed;
        private int _frames;
        private float _worst;

        public FrameSampler(float interval)
        {
            if (interval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(interval), "A sampling window has to last.");
            }

            _interval = interval;
        }

        public float Fps { get; private set; }
        public float AverageMs { get; private set; }
        public float WorstMs { get; private set; }

        public bool Add(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            _elapsed += deltaTime;
            _frames++;

            if (deltaTime > _worst)
            {
                _worst = deltaTime;
            }

            if (_elapsed < _interval)
            {
                return false;
            }

            Fps = _frames / _elapsed;
            AverageMs = _elapsed * 1000f / _frames;
            WorstMs = _worst * 1000f;

            _elapsed = 0f;
            _frames = 0;
            _worst = 0f;

            return true;
        }
    }
}
