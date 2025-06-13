using Bridge;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.SettingsPanel
{
    public class SettingsPanelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _environmentText;

        [Inject] private IBridge _bridge;
        
        private void OnEnable()
        {
            _environmentText.text = $"Environment: {_bridge.Environment.ToString()}";
        }
    }
}