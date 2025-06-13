using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AudioPanel
{
    internal sealed class OpenLevelEditorAudioPanelButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [Inject] private PostRecordEditorPageModel _postLevelEditorPageModel;
        
        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            _postLevelEditorPageModel?.OnOpenLevelAudioPanelClicked();
        }

        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }
    }
}
