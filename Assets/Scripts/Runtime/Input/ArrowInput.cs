using Game.Core;

namespace Game.Runtime
{
    // Holds the last arrow that was tapped until the run reads it. A button is
    // an event and the run asks a question once a frame, so something has to
    // remember the press in between; one press is enough, because a second one
    // inside the same frame is the player changing their mind.
    public sealed class ArrowInput : IInputSource
    {
        private Direction _pressed;
        private bool _hasPress;

        public void Press(Direction direction)
        {
            _pressed = direction;
            _hasPress = true;
        }

        public bool TryRead(out Direction direction)
        {
            direction = _pressed;

            if (!_hasPress)
            {
                return false;
            }

            _hasPress = false;

            return true;
        }

        public void Clear()
        {
            _hasPress = false;
        }
    }
}
