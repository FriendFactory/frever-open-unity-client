using JetBrains.Annotations;
using Navigation.Args;

namespace Modules.PageLoadTracking
{
    [UsedImplicitly]
    internal sealed class LevelEditorPageLoadTimeTracker : PageLoadTimeTrackerBase<LevelEditorArgs>
    {
        public override LoadTimeTrackerType Type => LoadTimeTrackerType.LevelEditor;
        
        protected override void OnTrackingStarted(LevelEditorArgs pageArgs)
        {
            pageArgs.OnLevelEditorLoaded += OnLevelEditorLoaded;

            void OnLevelEditorLoaded()
            {
                if (pageArgs == null) return;

                pageArgs.OnLevelEditorLoaded -= OnLevelEditorLoaded;

                OnTrackingEnded(pageArgs);
            }
        }
    }
}