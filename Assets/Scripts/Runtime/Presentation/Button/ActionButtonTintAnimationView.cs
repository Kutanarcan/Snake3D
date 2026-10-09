using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Runtime
{
    [RequireComponent(typeof(ActionButtonView))]
    public class ActionButtonTintAnimationView : MonoBehaviour
    {
        [SerializeField] private Graphic targetGraphic;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);

        [Header("Transition")]
        [SerializeField, Min(0f)] private float duration = 0.1f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        [SerializeField] private bool ignoreTimeScale = true;

        private ActionButtonView button;
        private Tween colorTween;
        private TweenCallback onColorTweenKill;
        private Color currentTarget;
        private bool isPressed;

        private void Reset()
        {
            targetGraphic = GetComponentInChildren<Graphic>();
        }

        private void Awake()
        {
            button = GetComponent<ActionButtonView>();
        }

        private void OnEnable()
        {
            button.PointerDown += HandlePointerDown;
            button.PointerUp += HandlePointerUp;
            button.InteractableChanged += HandleInteractableChanged;

            isPressed = false;

            currentTarget = GetTargetColor();
            KillTween();

            if (targetGraphic != null)
                targetGraphic.color = currentTarget;
        }

        private void OnDisable()
        {
            button.PointerDown -= HandlePointerDown;
            button.PointerUp -= HandlePointerUp;
            button.InteractableChanged -= HandleInteractableChanged;

            KillTween();
        }

        private void OnDestroy()
        {
            KillTween();
        }

        public void Refresh()
        {
            if (targetGraphic == null)
                return;

            Color next = GetTargetColor();

            if (next == currentTarget)
                return;

            currentTarget = next;

            KillTween();

            if (duration <= 0f)
            {
                targetGraphic.color = next;
                return;
            }

            colorTween = targetGraphic
                .DOColor(next, duration)
                .SetEase(ease)
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnKill(onColorTweenKill ??= HandleColorTweenKill);
        }

        private Color GetTargetColor()
        {
            if (!button.Interactable)
                return disabledColor;

            if (isPressed)
                return pressedColor;

            return normalColor;
        }

        private void KillTween()
        {
            colorTween?.Kill();
            colorTween = null;
        }

        private void HandleColorTweenKill() => colorTween = null;

        private void HandleInteractableChanged(bool interactable)
        {
            if (!interactable)
                isPressed = false;

            Refresh();
        }

        private void HandlePointerDown(PointerEventData eventData)
        {
            isPressed = true;
            Refresh();
        }

        private void HandlePointerUp(PointerEventData eventData)
        {
            isPressed = false;
            Refresh();
        }
    }
}
