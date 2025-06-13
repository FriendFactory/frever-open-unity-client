using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class IsMusicEnabledToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _musicEnabledGameObject;
        [Inject] private ILevelManager _levelManager;

        private void OnEnable()
        {
            Refresh();
            _levelManager.SongChanged += Refresh;
        }

        private void OnDisable()
        {
            _levelManager.SongChanged -= Refresh;
        }

        private void Refresh()
        {
            _musicEnabledGameObject.SetActive(_levelManager.IsSongSelected);
        }
    }
}
