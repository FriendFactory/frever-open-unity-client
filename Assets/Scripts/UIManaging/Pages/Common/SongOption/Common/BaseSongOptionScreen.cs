using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Common
{
    public abstract class BaseSongOptionScreen : MonoBehaviour
    {
        private SongOptionScreenOpener _opener;
        
        public void Show()
        {
            gameObject.SetActive(true);
            if (_opener != null)
            {
                _opener.gameObject.SetActive(false);
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            if (_opener != null)
            {
                _opener.gameObject.SetActive(true);
            }
        }

        internal void SetOpener(SongOptionScreenOpener opener)
        {
            _opener = opener;
        }
    }
}
