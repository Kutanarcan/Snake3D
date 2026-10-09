using Game.Core;

namespace Game.Tests.EditMode
{
    // Holds the direction a test asked for until it is read, exactly the way a
    // real source holds the last press until the next frame picks it up.
    public sealed class ScriptedInputSource : IInputSource
    {
        private Direction _pending;
        private bool _hasPending;

        public int ReadCount { get; private set; }

        public int ClearCount { get; private set; }

        public void Turn(Direction direction)
        {
            _pending = direction;
            _hasPending = true;
        }

        public bool TryRead(out Direction direction)
        {
            ReadCount++;
            direction = _pending;

            if (!_hasPending)
            {
                return false;
            }

            _hasPending = false;
            return true;
        }

        public void Clear()
        {
            ClearCount++;
            _hasPending = false;
        }
    }
}
