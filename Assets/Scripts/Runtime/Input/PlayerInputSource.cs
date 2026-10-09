using System;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Runtime
{
    public sealed class PlayerInputSource : IInputSource
    {
        private const float Deadzone = 0.5f;

        private readonly InputAction _move;
        private readonly ScreenToBoard _axes;

        public PlayerInputSource(InputAction move, ScreenToBoard axes)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (axes == null)
            {
                throw new ArgumentNullException(nameof(axes));
            }

            _move = move;
            _axes = axes;
        }

        public bool TryRead(out Direction direction)
        {
            Vector2 value = _move.ReadValue<Vector2>();

            float boardX;
            float boardY;
            _axes.Map(value.x, value.y, out boardX, out boardY);

            return DirectionExtensions.TryFromVector(boardX, boardY, Deadzone, out direction);
        }

        public void Clear()
        {
        }
    }
}
