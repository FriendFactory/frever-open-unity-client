using Extensions;
using Modules.Amplitude;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class VoiceAssetButton : BaseAssetButton
    {
        [SerializeField] private Image _icon;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _levelManager.EventLoadingCompleted += UpdateActiveStatus;
            _levelEditorPageModel.SongSelectionClosed += UpdateActiveStatus;

            UpdateActiveStatus();
        }

        protected override void OnDisable()
        {
            _levelManager.EventLoadingCompleted -= UpdateActiveStatus;
            _levelEditorPageModel.SongSelectionClosed -= UpdateActiveStatus;
            
            base.OnDisable();
        }
        
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnVoiceButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VOICE_FILTER_BUTTON_CLICKED);
        }

        private void UpdateActiveStatus()
        {
            var isActive = !_levelManager.TargetEvent?.HasMusic() ?? true;
            
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, isActive ? 1 : 0.5f);
            Interactable = isActive;
        }
    }
}