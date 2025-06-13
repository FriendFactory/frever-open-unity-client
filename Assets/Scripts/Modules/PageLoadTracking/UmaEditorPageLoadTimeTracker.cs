using JetBrains.Annotations;
using Navigation.Args;

namespace Modules.PageLoadTracking
{
    [UsedImplicitly]
    internal sealed class UmaEditorPageLoadTimeTracker: PageLoadTimeTrackerBase<UmaEditorArgs>
    {
        public override LoadTimeTrackerType Type => LoadTimeTrackerType.UmaEditor;

        protected override void OnTrackingStarted(UmaEditorArgs pageArgs)
        {
            pageArgs.LoadCompleteAction += OnUmaEditorLoaded;

            void OnUmaEditorLoaded()
            {
                if (pageArgs == null) return;

                pageArgs.LoadCompleteAction -= OnUmaEditorLoaded;

                OnTrackingEnded(pageArgs);
            }
        }
    }
}