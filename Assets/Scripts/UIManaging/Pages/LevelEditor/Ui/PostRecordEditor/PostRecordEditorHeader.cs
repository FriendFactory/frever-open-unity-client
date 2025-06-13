using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class PostRecordEditorHeader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [Inject] private PostRecordEditorPageModel _editorPageModel;

        private void OnEnable()
        {
            _editorPageModel.OutfitPanelOpened += OnOutfitPanelOpened;
            _editorPageModel.BodyAnimationsButtonClicked += OnBodyAnimationsButtonClicked;
            _editorPageModel.VfxButtonClicked += OnVfxButtonClicked;
            _editorPageModel.SetLocationsButtonClicked += OnSetLocationsButtonClicked;
            _editorPageModel.CameraButtonClicked += OnCameraButtonClicked;
            _editorPageModel.VoiceButtonClicked += OnVoiceFilterClicked;
            _editorPageModel.CameraFilterClicked += OnCameraCameraFilterClicked;
            _editorPageModel.SwitchCharacterButtonClicked += OnSwitchCharacterButtonClicked;
            _editorPageModel.AssetSelectionViewClosed += OnAssetSelectionViewClosed;
        }

        private void OnOutfitPanelOpened()
        {
            _headerText.text = "Change Look";
        }
        
        private void OnBodyAnimationsButtonClicked()
        {
            _headerText.text = "Add Animation";
        }
        
        private void OnVfxButtonClicked()
        {
            _headerText.text = "Add Effects";
        }
        
        private void OnSetLocationsButtonClicked()
        {
            _headerText.text = "Change Scenery";
        }
        
        private void OnCameraButtonClicked()
        {
            _headerText.text = "Camera";
        }
        
        private void OnVoiceFilterClicked()
        {
            _headerText.text = "Use voice effects";
        }

        private void OnCameraCameraFilterClicked()
        {
            _headerText.text = "Add Filters";
        }

        private void OnSwitchCharacterButtonClicked()
        {
            _headerText.text = "Change video members";
        }

        private void OnAssetSelectionViewClosed()
        {
            _headerText.text = "Edit Clips";
        }

        private void OnDisable()
        {
            _editorPageModel.OutfitPanelOpened -= OnOutfitPanelOpened;
            _editorPageModel.BodyAnimationsButtonClicked -= OnBodyAnimationsButtonClicked;
            _editorPageModel.VfxButtonClicked -= OnVfxButtonClicked;
            _editorPageModel.SetLocationsButtonClicked -= OnSetLocationsButtonClicked;
            _editorPageModel.CameraButtonClicked -= OnCameraButtonClicked;
            _editorPageModel.VoiceButtonClicked -= OnVoiceFilterClicked;
            _editorPageModel.CameraFilterClicked -= OnCameraCameraFilterClicked;
            _editorPageModel.SwitchCharacterButtonClicked -= OnSwitchCharacterButtonClicked;
            _editorPageModel.AssetSelectionViewClosed -= OnAssetSelectionViewClosed;
        }

        private void Reset()
        {
            _headerText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}
