namespace Modules.AudioControllers.Song
{
    public abstract class SongCondition
    {
        public abstract bool CheckCondition();
        public abstract bool Subscribe();
    }
}