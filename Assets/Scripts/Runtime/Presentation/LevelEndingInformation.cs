using System;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    [Serializable]
    public sealed class LevelEndingInformation
    {
        [SerializeField] private string _title;
        [SerializeField] private Sprite _star;
        [SerializeField] private string _buttonLabel;

        public string Title { get { return _title; } }
        public Sprite Star { get { return _star; } }
        public string ButtonLabel { get { return _buttonLabel; } }

        public Result Validate(string ending)
        {
            if (string.IsNullOrWhiteSpace(_title))
            {
                return Result.Failure("The " + ending + " ending has no title.");
            }

            if (_star == null)
            {
                return Result.Failure("The " + ending + " ending has no star icon.");
            }

            if (string.IsNullOrWhiteSpace(_buttonLabel))
            {
                return Result.Failure("The " + ending + " ending has no button label.");
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public LevelEndingInformation()
        {
        }

        public LevelEndingInformation(string title, Sprite star, string buttonLabel)
        {
            _title = title;
            _star = star;
            _buttonLabel = buttonLabel;
        }
#endif
    }
}
