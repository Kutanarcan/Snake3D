using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(fileName = "EndingSettings", menuName = "Snake/Ending Settings")]
    public sealed class EndingSettings : ScriptableObject
    {
        [SerializeField] private WinEffect _win;
        [SerializeField] private DeathEffect[] _deaths;

        public WinEffect Win { get { return _win; } }

        public DeathEffect Find(EndingId id)
        {
            if (_deaths == null || id == EndingId.None)
            {
                return null;
            }

            for (int i = 0; i < _deaths.Length; i++)
            {
                if (_deaths[i] != null && _deaths[i].Id == id)
                {
                    return _deaths[i];
                }
            }

            return null;
        }

        public Result Validate()
        {
            if (_win == null)
            {
                return Result.Failure("EndingSettings has no win effect.");
            }

            if (_deaths == null)
            {
                return Result.Success();
            }

            for (int i = 0; i < _deaths.Length; i++)
            {
                Result row = ValidateRow(i);
                if (!row.IsSuccess)
                {
                    return row;
                }
            }

            return Result.Success();
        }

        private Result ValidateRow(int index)
        {
            DeathEffect death = _deaths[index];

            if (death == null)
            {
                return Result.Failure("EndingSettings has an empty death at slot " + index + ".");
            }

            if (death.Id == EndingId.None)
            {
                return Result.Failure("EndingSettings has a death at slot " + index + " that names no ending.");
            }

            for (int i = 0; i < index; i++)
            {
                if (_deaths[i] != null && _deaths[i].Id == death.Id)
                {
                    return Result.Failure("EndingSettings has two deaths for " + death.Id + ".");
                }
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(WinEffect win, DeathEffect[] deaths)
        {
            _win = win;
            _deaths = deaths;
        }
#endif
    }
}
