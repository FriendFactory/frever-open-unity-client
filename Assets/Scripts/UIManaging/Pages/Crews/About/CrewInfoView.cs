using System;
using System.Linq;
using Abstract;
using Extensions;
using Modules.Amplitude;
using Modules.Crew;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewInfoView : BaseContextDataView<CrewInfoModel> 
    {
        [SerializeField] private TMP_Text _crewName;
        [SerializeField] private TMP_Text _membersCounter;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _trophyScore;
        [SerializeField] private TMP_Text _languageName;
        [SerializeField] private TMP_Text _languageEmoji;
        
        [SerializeField] private LanguageResources _languageResources;
        
        [Space]
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;

        [Inject] private CrewService _crewService;
        [Inject] private PageManager _pageManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PopupManager _popupManager;

        private bool _isInitialized;

        private void OnEnable()
        {
            _skeletonBehaviour.SetActive(true);
            _skeletonBehaviour.Play();
            
            if (_isInitialized)
            {
                _skeletonBehaviour.FadeOut();
            }
        }

        protected override void OnInitialized()
        {
            _crewName.text = ContextData.CrewName;
            _membersCounter.text = $"{ContextData.MembersCount}/{ContextData.MaxMembersCount}";
            _description.text = ContextData.Description;

            _trophyScore.text = ContextData.TrophyScore.ToString();
            
            UpdateLanguage();
            
            _isInitialized = true;
            _skeletonBehaviour.FadeOut();
        }

        protected override void BeforeCleanup()
        {
            _languageName.SetActive(false);
            _languageEmoji.SetActive(false);
        }

        private async void UpdateLanguage()
        {
            _languageName.SetActive(ContextData.LanguageId.HasValue);
            _languageEmoji.SetActive(ContextData.LanguageId.HasValue);

            if (!ContextData.LanguageId.HasValue) return;
            
            var languages = await _crewService.GetCrewLanguages(default);
            _languageEmoji.text =
                _languageResources.LanguageToEmojiMapping
                                  .FirstOrDefault(item => item.LanguageId == ContextData.LanguageId.Value)?.LanguageEmoji;
            _languageName.text = languages.FirstOrDefault(language => language.Id == ContextData.LanguageId.Value).Name;
        }
    }
}