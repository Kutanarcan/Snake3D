namespace Game.Core
{
    public interface IInputSource
    {
        bool TryRead(out Direction direction);
        void Clear();
    }
}
