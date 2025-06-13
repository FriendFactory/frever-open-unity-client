using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    public class ChatDirectHeaderView: MonoBehaviour
    {
        [SerializeField] private Button _groupChatBtn;

        public Action GroupChatClicked { get; set; }

        private void OnEnable()
        {
            _groupChatBtn.onClick.AddListener(OnGroupChatClicked);
        }

        private void OnDisable()
        {
            _groupChatBtn.onClick.RemoveListener(OnGroupChatClicked);
            GroupChatClicked = null;
        }

        private void OnGroupChatClicked()
        {
            GroupChatClicked?.Invoke();
        }
    }
}