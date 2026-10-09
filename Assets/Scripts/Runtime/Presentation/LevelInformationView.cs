using Game.Core;
using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class LevelInformationView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _foodAmount;

        public void Show(int eaten, int goal)
        {
            _foodAmount.SetText($" {eaten}/{goal}");
        }

        public Result Validate()
        {
            if (_foodAmount == null)
            {
                return Result.Failure("LevelInformationView has no food amount label assigned.");
            }

            return Result.Success();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void Configure(TMP_Text foodAmount)
        {
            _foodAmount = foodAmount;
        }
#endif
    }
}
