using UnityEngine;
using Bridge.Models.Common;
using System;
using Bridge;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public abstract class UIItem : MonoBehaviour
    {
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                ChangeSelectionVisual();
            }
        }

        private bool _selected;

        public virtual bool IsLoading 
        {
            get => _isLoading; 
            set => _isLoading = value;
        }
        private bool _isLoading;

        protected abstract void ChangeSelectionVisual();
    }

    public abstract class EntityUIItem<T> : UIItem where T : IEntity
    {
        public event Action<T> ItemSelected;

        [SerializeField] protected GameObject _newIcon;

        private long ItemModelId { get; set; }

        public T Entity { get; private set; }
        protected IBridge _bridge;
        protected RectTransform _rectTransform;

        public virtual void Init(IBridge bridge)
        {
            _bridge = bridge;
            _rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Setup(T entity)
        {
            ItemModelId = entity.Id;
            Entity = entity;
            UpdateIsNew();
        }

        protected virtual void UpdateIsNew()
        {
            if (_newIcon) _newIcon.SetActive(false);
        }

        protected virtual void OnItemSelected()
        {
            ItemSelected?.Invoke(Entity);
        }

        protected void ClearSelectedEvent()
        {
            if (ItemSelected == null)
            {
                return;
            }

            foreach (var handler in ItemSelected.GetInvocationList())
            {
                ItemSelected -= handler as Action<T>;
            }
        }
    }
}
