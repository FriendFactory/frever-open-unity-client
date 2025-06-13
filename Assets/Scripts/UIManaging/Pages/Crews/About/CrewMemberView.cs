using System;
using System.Threading;
using Abstract;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewMemberView : BaseContextDataView<CrewMemberModel>
    {
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;
        
        [Space]
        [SerializeField] private TMP_Text _nickname;
        [SerializeField] private GameObject _onlineDot;
        [SerializeField] private RawImage _portrait;

        [Space] 
        [SerializeField] private Button _button;

        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _tokenSource;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
            _button.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            _skeletonBehaviour.Play();

            if (ContextData is null) return;
            _nickname.text = ContextData.Nickname;
            _onlineDot.SetActive(ContextData.IsOnline);

            if (ContextData.Portrait != null)
            {
                OnPortraitDownloadSuccess(ContextData.Portrait);
                return;
            }

            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.GroupId, 
                                                                     Resolution._128x128,
                                                                     OnPortraitDownloadSuccess,
                                                                     OnPortraitDownloadFailure, 
                                                                     _tokenSource.Token);
        }

        private void OnPortraitDownloadSuccess(Texture2D texture2D)
        {
            ContextData.SetPortraitTexture(texture2D);
            _portrait.texture = texture2D;
            _skeletonBehaviour.FadeOut();
        }

        private void OnPortraitDownloadFailure(string error)
        {
            Debug.LogWarning(error);
        }

        private void OnButtonClicked()
        {
            var args = new UserProfileArgs(ContextData.GroupId, ContextData.Nickname, true);
            _pageManager.MoveNext(args);
        }
    }
}