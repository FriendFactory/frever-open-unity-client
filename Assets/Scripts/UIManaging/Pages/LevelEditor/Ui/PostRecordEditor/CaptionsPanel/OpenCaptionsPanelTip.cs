using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    [RequireComponent(typeof(PostRecordEditorDefaultStateListener))]
    internal sealed class OpenCaptionsPanelTip: MonoBehaviour
    {
        [SerializeField] private CanvasGroup _targetGroup;
        [SerializeField] private Button _activateButton;
        
        [Inject] private PostRecordEditorPageModel _postRecordEditorPageModel;
        [Inject] private ILevelManager _levelManager;

        private void OnEnable()
        {
            _activateButton.onClick.AddListener(OnClick);
            _postRecordEditorPageModel.CaptionPanelClosed += OnCaptionPanelClosed;
            _postRecordEditorPageModel.PostRecordEditorEventSelectionChanged += RefreshState;
        }

        private void OnDisable()
        {
            _activateButton.onClick.RemoveListener(OnClick);
            _postRecordEditorPageModel.CaptionPanelClosed -= OnCaptionPanelClosed;
            _postRecordEditorPageModel.PostRecordEditorEventSelectionChanged -= RefreshState;
        }

        private void OnClick()
        {
            _postRecordEditorPageModel.TryToOpenNewCaptionAddingPanel();
        }

        private void RefreshState(Event ev)
        {
            var hasCaption = ev.HasCaption();
            _targetGroup.SetActive(!hasCaption);
        }

        private void OnCaptionPanelClosed()
        {
            RefreshState(_levelManager.TargetEvent);
        }
    }
}