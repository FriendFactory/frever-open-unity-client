using System;
using System.Collections.Generic;
using Abstract;
using Bridge.Models.VideoServer;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.TaggedUsers
{
    public class TaggedUsersView : BaseContextDataView<TaggedGroup[]>
    {
        [Space]
        [SerializeField] private Button _overlayButton;
        [SerializeField] private Button _closeButton;
        [Space] 
        [SerializeField] private RectTransform _usersContainer;
        [SerializeField] private FeedTaggedUserItemView _taggedUserViewPrefab;

        public event Action CloseRequested; 

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _overlayButton.onClick.AddListener(OnOverlayButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);

            ShowTaggedUsers(ContextData);
        }

        protected override void BeforeCleanup()
        {
            RemoveChildren(_usersContainer.transform);
            base.BeforeCleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnOverlayButtonClicked()
        {
            CloseRequested?.Invoke();
        }

        private void OnCloseButtonClicked()
        {
            CloseRequested?.Invoke();
        }

        private void ShowTaggedUsers(IEnumerable<TaggedGroup> taggedGroups)
        {
            foreach (var taggedGroup in taggedGroups)
            {
                var taggedUserItemView = Instantiate(_taggedUserViewPrefab, _usersContainer);
                taggedUserItemView.Initialize(new FeedTaggedUserItemModel(taggedGroup.GroupId, taggedGroup.GroupNickname));
            }
        }

        private void RemoveChildren(Transform targetTransform)
        {
            for (int i = 0; i < targetTransform.childCount; i++)
            {
                Destroy(targetTransform.GetChild(i).gameObject);
            }
        }
    }
}