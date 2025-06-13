using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    /// <summary>
    ///     Build part of event based on template event data, such as SetLocationController, MusicController etc.
    ///     Interface is useful for injection (inject all steps as array)
    /// </summary>
    internal interface IEventBuildStep
    {
        void Run(BuildStepArgs buildStepArgs);
    }
    
    internal abstract class EventBuildStep : IEventBuildStep
    {
        private BuildStepArgs Args { get; set; }
        protected Event Template => Args.Template;
        protected Event Destination => Args.TargetEvent;
        protected ReplaceCharacterData[] ReplaceCharactersData => Args.ReplaceCharactersData;

        public void Run(BuildStepArgs buildStepArgs)
        {
            Args = buildStepArgs;
            RunInternal();
        }

        protected abstract void RunInternal();
    }

    internal struct BuildStepArgs
    {
        public Event Template;
        public ReplaceCharacterData[] ReplaceCharactersData;
        public Event TargetEvent;
    }
}