using System;
using Game.Core;

namespace Game.Runtime
{
    public sealed class InputOrchestrator : IInputSource
    {
        private readonly IInputSource[] _sources;

        public InputOrchestrator(IInputSource[] sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null)
                {
                    throw new ArgumentNullException(nameof(sources), "Input source " + i + " is missing.");
                }
            }

            _sources = sources;
        }

        public bool TryRead(out Direction direction)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i].TryRead(out direction))
                {
                    return true;
                }
            }

            direction = Direction.Up;

            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                _sources[i].Clear();
            }
        }
    }
}
