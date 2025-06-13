using System.Linq;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    /// <summary>
    /// Prevents blocking of UI clicking on UI elements behind the PiP background(such as captions) when it is needed
    /// </summary>
    internal sealed class BackgroundImageControl: StateListenerBase<PostRecordEditorState>
    {
        [SerializeField] private Image _image;
        [SerializeField] private PostRecordEditorState[] _unblockUiRaycastStates = { PostRecordEditorState.Default };
       

        protected override void OnInitialize()
        {
            EventsSource.RegisterListener(this);
        }

        public override void OnStateChanged(PostRecordEditorState state)
        {
            _image.raycastTarget = !_unblockUiRaycastStates.Contains(state);
        }
    }
}