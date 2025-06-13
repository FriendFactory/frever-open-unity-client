using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using Modules.Crew;
using TMPro;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    public class CrewItemView : BaseContextDataView<CrewShortInfo>
    {
        public event Action<CrewShortInfo> OnClick;

        [SerializeField] private ThumbnailLoader _crewPhoto;
        [SerializeField] private GameObject _privacyIcon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _membersCountText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _languageText;
        [SerializeField] private Button _button;
        [SerializeField] private FollowerMembersView _followerMembersView;

        [Inject] private CrewService _crewService;

        private CancellationTokenSource _tokenSource;
        
        private void Awake()
        {
            if (_button) _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            if (_tokenSource == null) return;
            
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
        }

        protected override async void OnInitialized()
        {
            _privacyIcon.SetActive(!ContextData.IsPublic);
            _nameText.text = ContextData.Name;
            _descriptionText.text = ContextData.Description;
            _descriptionText.SetActive(string.IsNullOrEmpty(ContextData.Description));
            _membersCountText.text = $"{ContextData.MembersCount}/{ContextData.TotalMembersCount}";
            _scoreText.text = ContextData.TrophyScore.ToString() ?? string.Empty;
            _crewPhoto.Initialize(ContextData);
             
            ProjectContext.Instance.Container.InjectGameObject(_followerMembersView.gameObject);
            ProjectContext.Instance.Container.InjectGameObject(gameObject);
            _tokenSource = new CancellationTokenSource();
            await SetupLanguageText();
            _followerMembersView.Initialize(new FollowerMembersModel
            {
                Members = ContextData.Members,
                FriendsCount = ContextData.FriendsCount,
                FollowersCount = ContextData.FollowersCount,
                FollowingCount = ContextData.FollowingCount
            });
        }
        
        private void OnButtonClick()
        {
            OnClick?.Invoke(ContextData);
        }

        private async Task SetupLanguageText()
        {
            if (ContextData.LanguageId is null)
            {
                _languageText.SetActive(false);
                return;
            }
            
            var languages = await _crewService.GetCrewLanguages(_tokenSource.Token);
            _languageText.text = languages.FirstOrDefault(l => l.Id == ContextData.LanguageId.Value)?.Name ?? "";
            _languageText.SetActive(true);
        }
    }
}