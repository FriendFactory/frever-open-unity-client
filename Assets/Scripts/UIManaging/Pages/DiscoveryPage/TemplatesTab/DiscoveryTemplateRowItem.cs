using System;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Templates;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    public class DiscoveryTemplateRowItem : TemplateRowItem
    {
        [SerializeField] private OpenTemplateButton _openTemplateButton;
        [SerializeField] private TMP_Text _creatorNameText;
        [SerializeField] private Button _creatorNameButton;
        
        [Inject] private PageManager _pageManager;
        [Inject] private DiscoveryPageLocalization _localization;
        
        private void Awake()
        {
            if (_creatorNameButton) _creatorNameButton.onClick.AddListener(OnCreatorNameClick);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _openTemplateButton.Setup(ContextData);
            if (_creatorNameText) _creatorNameText.text = $"@{ContextData.Creator.Nickname}";
        }

        protected override void UpdateUsagesText()
        {
            _usages.text = string.Format(_localization.SoundUsedCounterTextFormat, ContextData.UsageCount.ToShortenedString());
        }
        
        private void OnCreatorNameClick()
        {
            _pageManager.MoveNext(new UserProfileArgs(ContextData.CreatorId, string.Empty));
        }
    }
}