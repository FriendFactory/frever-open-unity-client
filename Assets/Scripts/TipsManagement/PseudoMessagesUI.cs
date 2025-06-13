using System;
using System.Collections.Generic;
using UnityEngine;

namespace TipsManagment
{
    public class PseudoMessagesUI : MonoBehaviour
    {
        [SerializeField] private PseudoMessageItem _messagePrefab;
        [SerializeField] private TextTip _tip;
        [SerializeField] private string _separator = Environment.NewLine;

        private List<PseudoMessageItem> _messageItems = new List<PseudoMessageItem>();

        void Start()
        {
            CreatePseudoMessages();
        }

        private void CreatePseudoMessages()
        {
            var messages = _tip.Text.Split(_separator.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                var newMessage = Instantiate(_messagePrefab, transform);
                newMessage.Init(message);

                _messageItems.Add(newMessage);
            }
        }
    }
}
