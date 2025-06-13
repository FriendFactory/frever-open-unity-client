using System;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LevelEditorVisibilityChanger : VisibilityChanger<LevelEditorState>
    {
        [Inject] private LevelEditorPageModel _pageModel;

        protected override void StartListeningStateChanging(Action<LevelEditorState> onStateChanged)
        {
            _pageModel.StateChanged += onStateChanged;
        }

        protected override void StopListeningStateChanging(Action<LevelEditorState> onStateChanged)
        {
            _pageModel.StateChanged -= onStateChanged;
        }
    }
}