using System;
using Game.Core;

namespace Game.Runtime
{
    public sealed class BoardPresentation
    {
        private readonly BoardView _board;
        private readonly GroundView _ground;
        private readonly BoardCamera _camera;

        public BoardPresentation(BoardView board, GroundView ground, BoardCamera camera)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (ground == null)
            {
                throw new ArgumentNullException(nameof(ground));
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _board = board;
            _ground = ground;
            _camera = camera;
        }

        public void Show(Level level)
        {
            _board.Show(level.Board);
            _ground.Show(level);
            _camera.Frame(level.Board);
        }
    }
}
