using System;
using Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Runtime
{
    public sealed class ArrowView : MonoBehaviour
    {
        [SerializeField] private ActionButtonView _button;
        [SerializeField] private Direction _direction;

        private ArrowInput _input;

        public Direction Direction
        {
            get { return _direction; }
        }

        public void Initialize(ArrowInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (_input != null)
            {
                _button.PointerDown -= OnPressed;
            }

            _input = input;
            _button.PointerDown += OnPressed;
        }

        public Result Validate()
        {
            if (_button == null)
            {
                return Result.Failure("ArrowView " + name + " has no button assigned.");
            }

            return Result.Success();
        }

        private void OnDestroy()
        {
            if (_input == null)
            {
                return;
            }

            _button.PointerDown -= OnPressed;
            _input = null;
        }

        private void OnPressed(PointerEventData eventData)
        {
            _input.Press(_direction);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(ActionButtonView button, Direction direction)
        {
            _button = button;
            _direction = direction;
        }
#endif
    }
}
