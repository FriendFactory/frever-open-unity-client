using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class CancelPreviewButton: MonoBehaviour
    {
        [Inject] private ILevelManager _levelManager;

        public bool IsShown => gameObject.activeInHierarchy;
        
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(CancelPreview);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void CancelPreview()
        {
            _levelManager.CancelPreview(PreviewCleanMode.KeepFirstEvent);
        }
    }
}