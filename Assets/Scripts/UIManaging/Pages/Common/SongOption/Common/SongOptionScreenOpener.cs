using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.Common
{
    [RequireComponent(typeof(Button))]
    public class SongOptionScreenOpener : MonoBehaviour
    {
        [SerializeField] private BaseSongOptionScreen _targetScreen;
        [SerializeField] private SongOptionScreenSwitcher _switcher;
        
        private Button _button;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Open);
            _targetScreen.SetOpener(this);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Open()
        {
            _switcher.Show(_targetScreen);
        }
    }
}