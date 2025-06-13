using System;
using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using Modules.LevelManaging.Editing;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal class AudioRecordingStateBasedColorSwitcher: BaseContextlessPanel 
    {
        [Serializable]
        protected class Settings
        {
            public AudioRecordingState state;
            public Color color = Color.white;
        }

        [SerializeField] private bool _initializeOnStart = true; 
        [SerializeField] private List<Graphic> _targets; 
        [SerializeField] private List<Settings> _settings;
        
        [Inject] private AudioRecordingStateController _stateController;

        private void Start()
        {
            if (_initializeOnStart)
            {
                Initialize();
            }
        }

        protected override void OnInitialized()
        {
            UpdateColor(_stateController.State);
            
            _stateController.RecordingStateChanged += OnRecordingStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            _stateController.RecordingStateChanged -= OnRecordingStateChanged;
        }

        private void OnRecordingStateChanged(AudioRecordingState source, AudioRecordingState destination)
        {
            UpdateColor(destination);
        }

        private void UpdateColor(AudioRecordingState state)
        {
            var settings = _settings.FirstOrDefault(setting => setting.state == state);
            
            if (settings == null) return;
            
            SetColor(settings.color);
        }

        private void SetColor(Color color)
        {
            _targets.ForEach(target => target.color = color);
        }
    }
}