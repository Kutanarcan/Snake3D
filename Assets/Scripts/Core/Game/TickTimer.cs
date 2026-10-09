using System;

namespace Game.Core
{
    public sealed class TickTimer
    {
        private const int MaxCatchUpTicks = 4;

        private float _interval;
        private float _counter;

        public TickTimer(float interval)
        {
            Interval = interval;
        }

        public float Interval
        {
            get { return _interval; }
            set
            {
                if (value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Tick interval must be positive.");
                }

                _interval = value;
            }
        }

        public float NormalizedTick
        {
            get { return _counter / _interval; }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            _counter += deltaTime;

            float ceiling = _interval * MaxCatchUpTicks;
            if (_counter > ceiling)
            {
                _counter = ceiling;
            }
        }

        public bool TryConsumeTick()
        {
            if (_counter < _interval)
            {
                return false;
            }

            _counter -= _interval;
            return true;
        }

        public void Reset()
        {
            _counter = 0f;
        }
    }
}
