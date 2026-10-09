using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class PlayerPrefsLevelPersistent : ILevelPersistent
    {
        public const string Key = "snake.level";

        public int Read()
        {
            return PlayerPrefs.GetInt(Key, 0);
        }

        public void Write(int index)
        {
            PlayerPrefs.SetInt(Key, index);
            PlayerPrefs.Save();
        }
    }
}
