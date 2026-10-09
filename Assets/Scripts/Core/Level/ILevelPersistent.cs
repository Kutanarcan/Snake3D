namespace Game.Core
{
    public interface ILevelPersistent
    {
        int Read();
        void Write(int index);
    }
}
