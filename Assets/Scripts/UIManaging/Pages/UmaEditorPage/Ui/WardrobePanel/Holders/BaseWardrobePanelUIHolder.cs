using UnityEngine;
using Bridge;
using Modules.WardrobeManaging;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public abstract class BaseWardrobePanelUIHolder : MonoBehaviour 
    {
        [SerializeField]
        protected RectTransform _content;
        [SerializeField]
        protected GameObject _itemPrefab;

        public bool IsShown => gameObject.activeInHierarchy;
        [Inject]
        protected IBridge _bridge;
        [Inject]
        protected ClothesCabinet _clothesCabinet;

        private void Start(){}

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void Clear();
    }
}
