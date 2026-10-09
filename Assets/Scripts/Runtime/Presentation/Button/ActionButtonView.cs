using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Game.Runtime
{
    [DisallowMultipleComponent]
    public class ActionButtonView : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        [FormerlySerializedAs("<Interactable>k__BackingField")]
        [SerializeField] private bool _interactable = true;

        public event Action<PointerEventData> PointerDown;
        public event Action<PointerEventData> PointerUp;
        public event Action Click;

        public event Action<bool> InteractableChanged;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                if (_interactable == value)
                    return;

                _interactable = value;
                InteractableChanged?.Invoke(value);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            PointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            PointerUp?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            Click?.Invoke();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
                InteractableChanged?.Invoke(_interactable);
        }
#endif
    }
}
