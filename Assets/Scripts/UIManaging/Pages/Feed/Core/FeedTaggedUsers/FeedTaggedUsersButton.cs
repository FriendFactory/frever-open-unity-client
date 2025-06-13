using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.VideoServer;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.Feed.Ui.Feed.FeedTaggedUsers
{
    public class FeedTaggedUsersButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _taggedUsersCounter;
        [SerializeField] private Button _taggedUsersButton;
        
        [SerializeField] private LocalizedString _taggedUsersButtonText;
        public event UnityAction<List<TaggedGroup>> OnClick;

        private TaggedGroup[] _taggedUsers;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Setup(TaggedGroup[] taggedUsers)
        {
            _taggedUsers = taggedUsers;
            _taggedUsersButton.gameObject.SetActive(taggedUsers != null && taggedUsers.Length > 0);
            if (taggedUsers != null)
            {
                _taggedUsersCounter.text = $"{taggedUsers.Length} {_taggedUsersButtonText}";
                _taggedUsersButton.onClick.RemoveListener(Clicked);
                _taggedUsersButton.onClick.AddListener(Clicked);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Clicked()
        {
            if (_taggedUsers == null || _taggedUsers.Length == 0)
            {
                Debug.LogException(new NullReferenceException(nameof(_taggedUsers)), this);
                return;
            }
            if (OnClick != null) OnClick.Invoke(_taggedUsers.ToList());
        }
    }
}