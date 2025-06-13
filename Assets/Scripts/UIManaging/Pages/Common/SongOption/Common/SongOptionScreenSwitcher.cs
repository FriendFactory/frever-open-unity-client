using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Common
{
    public class SongOptionScreenSwitcher : MonoBehaviour
    {
        private BaseSongOptionScreen _active;

        public void Show(BaseSongOptionScreen screen)
        {
            if (_active == screen) return;
            if (_active != null) _active.Hide();
            _active = screen;
            _active.Show();
        }

        public void Hide()
        {
            if (_active == null) return;
            _active.Hide();
            _active = null;
        }
    }
}
