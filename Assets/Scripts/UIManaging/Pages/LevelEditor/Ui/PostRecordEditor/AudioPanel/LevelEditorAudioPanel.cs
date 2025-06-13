using Extensions;
using Modules.InputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AudioPanel
{
    internal sealed class LevelEditorAudioPanel : MonoBehaviour
    {
        [SerializeField] private BaseAudioSlider _musicSlider;
        [SerializeField] private BaseAudioSlider _voiceSlider;
        [SerializeField] private GameObject _body;
        [SerializeField] private PointerDownButton _outOfViewEmptySpace;
        
        [Inject] private PostRecordEditorPageModel _postRecordEditorPageModel;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            Hide();
        }
        
        private void OnEnable()
        {
            _postRecordEditorPageModel.OpenLevelAudioPanelClicked += OnOpenLevelAudioPanelClicked;
        }

        private void OnDisable()
        {
            _postRecordEditorPageModel.OpenLevelAudioPanelClicked -= OnOpenLevelAudioPanelClicked;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void Show()
        {
            if(_body.activeSelf) return;

            _outOfViewEmptySpace.OnPointerDown += OnClickedOutsideOfView;
            _body.SetActive(true);
            
            var hasMusicInEvent = _levelManager.TargetEvent.HasMusic();
            _musicSlider.SetActive(hasMusicInEvent);
           
            var hasVoiceTrackInEvent = _levelManager.TargetEvent.HasVoiceTrack();
            _voiceSlider.SetActive(hasVoiceTrackInEvent);
            
            _backButtonEventHandler.AddButton(gameObject, ApplyAndClose);
        }

        private void Hide()
        {
            _outOfViewEmptySpace.OnPointerDown -= OnClickedOutsideOfView;
            _body.SetActive(false);
            
            _backButtonEventHandler.RemoveButton(gameObject);
        }
        
        private void OnClickedOutsideOfView(PointerEventData pointerData)
        {
            ApplyAndClose();
        }

        private void ApplyAndClose()
        {
            _musicSlider.ApplyCurrentValue();
            _voiceSlider.ApplyCurrentValue();
            
            Hide();
            _postRecordEditorPageModel.OnLevelAudioPanelClosed();
        }

        private void OnOpenLevelAudioPanelClicked()
        {
            Show();
            _postRecordEditorPageModel.OnLevelAudioPanelOpened();
        }
    }
}