using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Common.ProgressBars
{
    internal sealed class ProgressMessages : MonoBehaviour
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private TextMeshProUGUI _label;
        
        [SerializeField] private ProgressMessagesSettings _settings;

        private ProgressMessageData[] _messages => _settings.Messages;
        private float _currentMin, _currentMax;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        private void Awake()
        {
            // Ascending order is important
            Array.Sort(_messages, (a, b) => a.CompareTo(b));

            _progressBar.ValueChanged += OnProgressValueChanged;
        }

        private void Start()
        {
            ShowMessage(_messages?[0]);
        }

        private void OnDestroy()
        {
            _progressBar.ValueChanged -= OnProgressValueChanged;
        }

        //---------------------------------------------------------------------
        // UI Callbacks
        //---------------------------------------------------------------------
        private void OnProgressValueChanged(float value)
        {
            UpdateCurrentMessage(value);
        }

        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------
        private void ShowMessage(ProgressMessageData message)
        {
            if (message != null)
            {
                var index = Array.IndexOf(_messages, message);
                
                // Min value is taken from previous message or from 'Min' value of progress if no previous message found
                _currentMin = index > 0 ? _messages[index - 1].ProgressValue : _progressBar.Min;
                
                // Max value is taken from current message or from 'Max' value of progress if no next message found
                _currentMax = index < _messages.Length - 1 ? message.ProgressValue : _progressBar.Max;
                
                // Show message
                _label.text = message.Message;
            }
            else
            {
                _currentMin = _progressBar.Min;
                _currentMax = _progressBar.Max;
                _label.text = string.Empty;
            }
        }

        private void UpdateCurrentMessage(float progressValue)
        {
            if (progressValue < _currentMin || _currentMax < progressValue)
            {
                ShowMessage(_messages.FirstOrDefault(m => m.ProgressValue >= progressValue));
            }
        }
    }
}