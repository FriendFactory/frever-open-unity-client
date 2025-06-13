using System;
using Abstract;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal class ViewAllCrewMembersListItem : BaseContextDataView<ViewAllCrewMembersListItemModel>
    {
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;
        
        [Space]
        [SerializeField] private TMP_Text _place;
        [SerializeField] private RawImage _portrait;
        [SerializeField] private GameObject _statusBall;
        [SerializeField] private TMP_Text _username;
        [SerializeField] private TMP_Text _score;
        
        [Space] 
        [SerializeField] private GameObject _highlight;

        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;

        private void OnEnable()
        {
            _skeletonBehaviour.Play();
        }

        protected override void OnInitialized()
        {
            ContextData.DataFetched += OnDataFetched;
            if (ContextData.Initialized == false) return;
            _highlight.SetActive(false);
            
            OnDataFetched();
        }

        private void OnThumbnailDownloaded(Texture2D thumbnail)
        {
            _portrait.texture = thumbnail;
            _skeletonBehaviour.FadeOut();
        }

        private void OnDataFetched()
        {
            _place.text = ContextData.Place;
            _thumbnailsDownloader.GetCharacterThumbnailByUserGroupId(ContextData.GroupId, Resolution._128x128, OnThumbnailDownloaded);
            _statusBall.SetActive(ContextData.IsOnline);
            _username.text = ContextData.Username;
            _score.text = ContextData.Score;
            _highlight.SetActive(ContextData.IsLocalUser);
        }
    }
}