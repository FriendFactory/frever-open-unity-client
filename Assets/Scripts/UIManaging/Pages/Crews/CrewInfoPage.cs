using System;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewInfoPage : GenericPage<CrewInfoPageArgs>
    {
        [SerializeField] private AboutTabContent _crewPanel;
        
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_Text _joinButtonText;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewPageLocalization _crewPageLocalization;
        
        private CrewModel _fullCrewModel;
        
        public override PageId Id => PageId.CrewInfo;
        
        protected override void OnInit(PageManager pageManager) { }

        protected override async void OnDisplayStart(CrewInfoPageArgs args)
        {
            base.OnDisplayStart(args);

            var result = await _bridge.GetCrew(args.CrewShortInfo.Id, default);

            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            _fullCrewModel = result.Model;
            
            _crewPanel.Initialize(result.Model);
            _crewPanel.InitFollowerMembers(args.CrewShortInfo);
            SetupJoinButton();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _crewPanel.CleanUp();
        }

        private async void SetupJoinButton()
        {
            _joinButton.interactable = false;
            _joinButton.onClick.RemoveAllListeners();
            _joinButtonText.text = _crewPageLocalization.JoinStateLoadingButton;

            await _userData.DownloadProfile();
            
            if (IsDestroyed) return;
            
            if (_userData.UserProfile.CrewProfile != null && _userData.UserProfile.CrewProfile.Id != _fullCrewModel.Id)
            {
                _joinButtonText.text = _crewPageLocalization.AlreadyInAnotherCrewJoinButton;
                return;
            }
            
            if (_fullCrewModel.MembersCount >= _fullCrewModel.TotalMembersCount)
            {
                _joinButtonText.text = _crewPageLocalization.JoinCrewIsFullButton;
                return;
            }

            if (_fullCrewModel.IsPublic)
            {
                _joinButton.interactable = true;
                _joinButtonText.text = _crewPageLocalization.JoinCrewButton;
                _joinButton.onClick.AddListener(JoinPublicCrew);
                return;
            }
            
            if (_fullCrewModel.IsJoinRequested)
            {
                _joinButtonText.text =  _crewPageLocalization.JoinRequestSentButton;
                return;
            }
            
            if (_fullCrewModel.IsInvited)
            {
                _joinButton.interactable = true;
                _joinButtonText.text = _crewPageLocalization.AcceptInvitationButton;
                _joinButton.onClick.AddListener(AcceptInvitation);
                return;
            }
            
            _joinButton.interactable = true;
            _joinButtonText.text = _crewPageLocalization.SendJoinRequestButton;
            _joinButton.onClick.AddListener(SendJoinRequest);
        }
        
        private async void JoinPublicCrew()
        {
            _joinButton.interactable = false;
            
            var result = await _bridge.SendJoinRequest(_fullCrewModel.Id, string.Empty);

            _joinButton.interactable = true;
            
            if (result.IsSuccess)
            {
                Manager.MoveNext(new CrewPageArgs());
            }
        }
        
        private async void SendJoinRequest()
        {
            _joinButton.interactable = false;
            
            var result = await _bridge.SendJoinRequest(_fullCrewModel.Id, string.Empty);

            if (result.IsSuccess)
            {
                _snackBarHelper.ShowInformationSnackBar(_crewPageLocalization.JoinRequestSentSnackbarMessage);
                _joinButtonText.text = _crewPageLocalization.JoinRequestSentButton;
            }
            else
            {
                _joinButton.interactable = true;
            }
        }

        private async void AcceptInvitation()
        {
            _joinButton.interactable = false;
            
            var result = await _bridge.AcceptCrewInvitation(_fullCrewModel.Id);

            _joinButton.interactable = true;
            
            if (result.IsSuccess)
            {
                Manager.MoveNext(new CrewPageArgs());
            }
        }
    }
}