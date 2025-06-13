using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Modules.LevelManaging.Assets
{
    internal sealed class VoiceFilterController : IVoiceFilterController
    {
        private const string MIXER_PATH = "AudioMixers/Character1";
        private const long DEFAULT_VOICE_FILTER_ID = 7;
        
        private AudioMixerSnapshot _voiceFilterSnapshot;
        private readonly AudioMixer _audioMixer;
        private readonly IDataFetcher _dataFetcher;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public VoiceFilterFullInfo NoVoiceFilter { get; private set; }
        public VoiceFilterFullInfo Current { get; private set; }
        public long NoVoiceFilterId => NoVoiceFilter.Id;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<DbModelType> VoiceFilterChanged;
        public event Action<VoiceFilterFullInfo> VoiceFilterChangedSilently;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VoiceFilterController(IDataFetcher dataFetcher)
        {
            _dataFetcher = dataFetcher;
            _audioMixer = Resources.Load<AudioMixer>(MIXER_PATH);
            _dataFetcher.OnDataFetched += OnDataFetched;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Change(VoiceFilterFullInfo voiceFilter, Action onSuccess, Action<string> onFail)
        {
            SetFilter(voiceFilter, onSuccess, onFail);
            VoiceFilterChanged?.Invoke(DbModelType.VoiceFilter);
        }

        public void ChangeSilently(VoiceFilterFullInfo voiceFilter, Action onSuccess, Action<string> onFail)
        {
            SetFilter(voiceFilter, onSuccess, onFail);
            VoiceFilterChangedSilently?.Invoke(voiceFilter);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetFilter(VoiceFilterFullInfo voiceFilter, Action onSuccess, Action<string> onFail)
        {
            _voiceFilterSnapshot = _audioMixer.FindSnapshot(voiceFilter.Name);
            if (_voiceFilterSnapshot == null)
            {
                onFail.Invoke("Couldn't find matching voice filter snapshot.");
            }

            Current = voiceFilter;
            _voiceFilterSnapshot?.TransitionTo(0.1f);
            onSuccess?.Invoke();
        }
        
        private void OnDataFetched()
        {
            NoVoiceFilter = _dataFetcher.VoiceFilters.FirstOrDefault(v => v.Id == DEFAULT_VOICE_FILTER_ID);
            _dataFetcher.OnDataFetched -= OnDataFetched;
        }
    }
}