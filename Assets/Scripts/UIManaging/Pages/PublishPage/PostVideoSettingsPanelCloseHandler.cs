using UIManaging.Pages.PublishPage.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage
{
    /// <summary>
    /// Helper class to handle PostVideoSettingsPanel closing. The panel class itself is overloaded with different logic,
    /// so, it is better to separate the closing logic into a separate class or refactor the whole panel class.
    /// </summary>
    [RequireComponent(typeof(VideoSettingsPanelBase))]
    internal sealed class VideoSettingsPanelCloseHandler: MonoBehaviour
    {
        [SerializeField] private Button _overlayButton;
        [SerializeField] private Button _saveButton;
        
        private VideoSettingsPanelBase _videoSettingsPanel;
        
        private void Awake()
        {
            _videoSettingsPanel = GetComponent<VideoSettingsPanelBase>();
        }
        
        private void OnEnable()
        {
            _overlayButton.onClick.AddListener(ClosePanel);
            _saveButton.onClick.AddListener(ClosePanel);
        }
        
        private void OnDisable()
        {
            _overlayButton.onClick.RemoveListener(ClosePanel);
            _saveButton.onClick.RemoveListener(ClosePanel);
        }
        
        private void ClosePanel()
        {
            _videoSettingsPanel.Hide();
        }
    }
}