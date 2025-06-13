using System;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class PostRecordEditorVisibilityChanger : VisibilityChanger<PostRecordEditorState>
    {
        [Inject] private PostRecordEditorPageModel _pageModel;

        protected override void StartListeningStateChanging(Action<PostRecordEditorState> onStateChanged)
        {
            _pageModel.StateChanged += onStateChanged;
        }

        protected override void StopListeningStateChanging(Action<PostRecordEditorState> onStateChanged)
        {
            _pageModel.StateChanged -= onStateChanged;
        }
    }
}