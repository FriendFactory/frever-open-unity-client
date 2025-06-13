using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    /// <summary>
    /// You should use to prevent record button pressing during some savings
    /// </summary>
    internal sealed class RecordButtonBlocker : MonoBehaviour
    {
        [SerializeField] private GameObject _recordButton;
        
        public void Switch(bool isActive)
        {
            _recordButton.SetActive(!isActive);
            gameObject.SetActive(isActive);
        }
    }
}
