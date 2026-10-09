using Game.Core;

namespace Game.Runtime
{
    public interface IEffectPlayer
    {
        void Play(EffectId id, GridPosition cell);
    }
}
