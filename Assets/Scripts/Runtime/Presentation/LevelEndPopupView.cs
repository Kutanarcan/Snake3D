using System;
using DG.Tweening;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime
{
    public sealed class LevelEndPopupView : MonoBehaviour
    {
        [SerializeField] private LevelEndPopupSettings _settings;
        [SerializeField] private Image _dim;
        [SerializeField] private Transform _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _star;
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private ActionButtonView _button;

        private Action _buttonPressedCallback;

        public void Initialize(Action buttonPressed)
        {
            if (buttonPressed == null)
            {
                throw new ArgumentNullException(nameof(buttonPressed));
            }

            if (_buttonPressedCallback != null)
            {
                _button.Click -= OnPressed;
            }

            _buttonPressedCallback = buttonPressed;
            _button.Click += OnPressed;
            _button.Interactable = false;
        }

        public void Show(GameStatus status)
        {
            LevelEndingInformation ending = _settings.GetLevelEndingInformation(status);

            _title.SetText(ending.Title);
            _star.sprite = ending.Star;
            _buttonLabel.SetText(ending.ButtonLabel);

            _button.Interactable = true;

            gameObject.SetActive(true);

            StopAnimating();
            _dim.color = Faded(0f);
            _panel.localScale = Vector3.one * _settings.FromScale;

            _dim.DOColor(Faded(_settings.DimAlpha), _settings.Duration)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            _panel.DOScale(Vector3.one, _settings.Duration)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        public void Hide()
        {
            _button.Interactable = false;

            StopAnimating();
            gameObject.SetActive(false);
        }

        public Result Validate()
        {
            Result parts = ValidateParts();
            if (!parts.IsSuccess)
            {
                return parts;
            }

            return _settings.Validate();
        }

        private Result ValidateParts()
        {
            if (_settings == null) { return Missing("settings"); }
            if (_dim == null) { return Missing("dim image"); }
            if (_panel == null) { return Missing("panel"); }
            if (_title == null) { return Missing("title label"); }
            if (_star == null) { return Missing("star image"); }
            if (_buttonLabel == null) { return Missing("button label"); }
            if (_button == null) { return Missing("button"); }

            return Result.Success();
        }

        private void OnDestroy()
        {
            if (_buttonPressedCallback == null)
            {
                return;
            }

            _button.Click -= OnPressed;
            _buttonPressedCallback = null;
        }

        private void OnPressed()
        {
            _buttonPressedCallback();
        }

        private static Result Missing(string field)
        {
            return Result.Failure("LevelEndPopupView has no " + field + " assigned.");
        }

        private void StopAnimating()
        {
            _dim.DOKill();
            _panel.DOKill();
        }

        private Color Faded(float alpha)
        {
            Color color = _dim.color;
            color.a = alpha;

            return color;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(LevelEndPopupSettings settings, Image dim, Transform panel, TMP_Text title,
            Image star, TMP_Text buttonLabel, ActionButtonView button)
        {
            _settings = settings;
            _dim = dim;
            _panel = panel;
            _title = title;
            _star = star;
            _buttonLabel = buttonLabel;
            _button = button;
        }
#endif
    }
}
