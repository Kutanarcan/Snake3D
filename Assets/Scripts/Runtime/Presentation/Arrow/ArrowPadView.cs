using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class ArrowPadView : MonoBehaviour
    {
        private const int Directions = 4;

        [SerializeField] private ArrowView[] _arrows;

        public void Initialize(ArrowInput input)
        {
            for (int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i].Initialize(input);
            }
        }

        public Result Validate()
        {
            if (_arrows == null || _arrows.Length != Directions)
            {
                return Result.Failure("ArrowPadView needs exactly " + Directions + " arrows.");
            }

            int covered = 0;

            for (int i = 0; i < _arrows.Length; i++)
            {
                if (_arrows[i] == null)
                {
                    return Result.Failure("ArrowPadView is missing the arrow at slot " + i + ".");
                }

                Result arrowResult = _arrows[i].Validate();
                if (!arrowResult.IsSuccess)
                {
                    return arrowResult;
                }

                int bit = 1 << (int)_arrows[i].Direction;
                if ((covered & bit) != 0)
                {
                    return Result.Failure("ArrowPadView points two arrows at " + _arrows[i].Direction + ".");
                }

                covered |= bit;
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(ArrowView[] arrows)
        {
            _arrows = arrows;
        }
#endif
    }
}
