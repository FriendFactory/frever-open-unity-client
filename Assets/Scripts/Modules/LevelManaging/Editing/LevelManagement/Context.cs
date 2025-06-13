using Models;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    /// <summary>
    ///  Provides main LevelManager state/data for internal services
    /// </summary>
    internal interface IContext
    {
        Level CurrentLevel { get; }
    }

    /// <summary>
    ///  Provides writing control over context, which should be available only from particular types
    /// </summary>
    internal interface IContextControl: IContext
    {
        void SetCurrentLevel(Level level);
    }
    
    internal sealed class Context: IContextControl
    {
        public Level CurrentLevel { get; private set; }
        
        public void SetCurrentLevel(Level level)
        {
            CurrentLevel = level;
        }
    }
}