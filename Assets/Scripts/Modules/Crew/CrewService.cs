using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.Common.Files;
using Bridge.Results;
using Bridge.Scripts.ClientServer.Chat;
using JetBrains.Annotations;
using Navigation.Core;
using Common;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using Zenject;

namespace Modules.Crew
{
    [UsedImplicitly]
    public sealed partial class CrewService
    {
        private const string MEMBER_NOT_FOUND_ERROR = "Member is not found or not accessible";
        private const string INSUFFICIENT_ACCESS_LEVEL_ERROR = "InsufficientAccessLevel";
        private const string NAME_TAKEN_ERROR = "is used already";
        
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private CrewPageLocalization _localization;
        
        private long _crewId;
        private CrewModel _crewModel;
        private LanguageInfo[] _languages;

        public bool LocalUserIsAdmin { get; private set; }
        public bool LocalUserIsLeader { get; private set; }
        public CrewMember LocalUserMemberData { get; private set; }
        public CrewModel Model => _crewModel;

        public event Action SidebarActivated;
        public event Action SidebarDisabled;
        
        public event Action<CrewModel> CrewModelUpdated;
        public event Action<IReadOnlyCollection<CrewMember>> MembersListUpdated;
        public event Action<string> MotDUpdated;
        public event Action<long> UserInvited;
        public event Action<long> LanguageUpdated;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void OpenCrewSidebar()
        {
            await RefreshCrewDataAsync(default);
            var config = new CrewSidebarPopupConfiguration(Model, OnSidebarSlideOut);
            ShowSideBar(config);
        }
        
        public void OpenJoinRequests()
        {
            var config = new CrewSidebarPopupConfiguration(Model, OnSidebarSlideOut, true);
            ShowSideBar(config);
        }

        public async void RefreshCrewData()
        {
            await RefreshCrewDataAsync(default);
        }

        public async Task RefreshCrewDataAsync(CancellationToken token)
        {
            if (_localUser.UserProfile == null)
            {
                await WaitForProfile();
            }
            
            if (_localUser.UserProfile.CrewProfile is null)
            {
                _crewModel = null;
                _crewId = default;
                
                return;
            }

            _crewId = _localUser.UserProfile.CrewProfile.Id;
            var result = await _bridge.GetCrew(_crewId, token);
            if (result.IsSuccess)
            {
                _crewModel = result.Model;
                
                RefreshCachedData();
                
                CrewModelUpdated?.Invoke(_crewModel);
                MembersListUpdated?.Invoke(_crewModel.Members);
                MotDUpdated?.Invoke(_crewModel.MessageOfDay);
                
                return;
            }
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);
        }

        public void TryKickMember(long memberId, string nickname, Action onSuccess)
        {
            _popupManagerHelper.ShowDialogPopup(_localization.KickUserPopupTitle, 
                                                string.Format(_localization.KickUserPopupDescription, nickname),
                                                _localization.KickUserPopupCancelButton,
                                                () => _popupManager.ClosePopupByType(PopupType.DialogDarkV3),
                                                _localization.KickUserPopupConfirmButton,
                                                OnKickConfirmed,
                                                true);
            return;

            async void OnKickConfirmed()
            {
                var result = await _bridge.RemoveCrewMember(_crewId, memberId);
                if (result.IsSuccess)
                {
                    var member = _crewModel.Members.FirstOrDefault(m => m.Group.Id == memberId);
                    if (member is null)
                    {
                        Debug.LogError($"Member {memberId} not found on the member list");
                    }

                    var newList = new List<CrewMember>(_crewModel.Members);
                    newList.Remove(member);
                    MembersListUpdated?.Invoke(newList);

                    _snackBarHelper.ShowInformationSnackBar(_localization.UserRemovedSnackbarMessageFormat);

                    await RefreshCrewDataAsync(default);
                    onSuccess?.Invoke();
                    _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
                    return;
                }

                if (!result.IsError) return;
                if (result.ErrorMessage.Contains(MEMBER_NOT_FOUND_ERROR))
                {
                    _snackBarHelper.ShowFailSnackBar(_localization.MemberNotFoundSnackbarMessage);
                    await RefreshCrewDataAsync(default);
                    _popupManager.ClosePopupByType(PopupType.ManageCrewMember);

                    return;
                }

                _snackBarHelper.ShowFailSnackBar(_localization.EditAccessRestrictedSnackbarMessage);
                await RefreshCrewDataAsync(default);
            }
        }
        
        public async Task UpdateMotD(string message)
        {
            var result = await _bridge.SetMessageOfTheDay(_crewId, message);
            if (result.IsSuccess)
            {
                _crewModel.MessageOfDay = message;
                _snackBarHelper.ShowSuccessDarkSnackBar(_localization.MotdPostedSnackbarMessage);
                MotDUpdated?.Invoke(message);
                return;
            }

            _snackBarHelper.ShowFailSnackBar(_localization.EditAccessRestrictedSnackbarMessage);
            await RefreshCrewDataAsync(default);
        }

        public void TryLeaveCrew(Action onSuccess, long? newLeaderId = null)
        {
            var groupId = _localUser.GroupId;
            var memberData = _crewModel.Members.FirstOrDefault(m => m.Group.Id == groupId);

            if (memberData is null)
            {
                Debug.LogError($"Couldn't find {groupId} in crew members");
                return;
            }
            
            if (memberData.RoleId == Constants.Crew.LEADER_ROLE_ID && _crewModel.Members.Length > 1)
            {
                _snackBarHelper.ShowFailSnackBar(_localization.TransferOwnershipFirstSnackbarMessage);
                return;
            }
            
            var config = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.LeavePopupTitle,
                YesButtonText = _localization.LeavePopupConfirmButton,
                YesButtonSetTextColorRed = true,
                NoButtonText = _localization.LeavePopupCancelButton,
                OnClose = OnLeavePopupClosed,
                Description = _localization.LeavePopupDescription
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);

            async void OnLeavePopupClosed(object userChoice)
            {
                var userLeft = userChoice is bool b && b;
                if (!userLeft) return;

                var result = await _bridge.LeaveCrew(_crewId, newLeaderId);
                if (result.IsSuccess)
                {
                    await _localUser.DownloadProfile();
                    onSuccess?.Invoke();
                    _snackBarHelper.ShowInformationSnackBar(_localization.LeftCrewSnackbarMessage);
                    return;
                }

                Debug.LogError(result.ErrorMessage);
            }
        }

        public void DeleteCrew(Action onSuccess)
        {
            if (_crewModel.Members.Length != 1)
            {
                ShowCantDeleteCrewPopup();
                return;
            }

            var cfg = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.DeleteCrewPopupTitle,
                Description = _localization.DeleteCrewPopupDescription,
                YesButtonText = _localization.DeleteCrewPopupConfirmButton,
                YesButtonSetTextColorRed = true,
                OnYes = OnDeleteActionRequested,
                NoButtonText = _localization.DeleteCrewPopupCancelButton,
                OnNo = () => _popupManager.ClosePopupByType(PopupType.DialogDarkV3)
            };
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType, true);

            async void OnDeleteActionRequested()
            {
                var result = await _bridge.LeaveCrew(_crewId);
                if (result.IsSuccess)
                {
                    onSuccess.Invoke();

                    return;
                }

                if (result.IsError) Debug.LogError(result.ErrorMessage);
            }
        }

        public async Task<bool> UpdateCrewNameAsync(string name)
        {
            var isSuccess = await SaveChangesToCrewModel(name);
            if (!isSuccess) return false;
            
            _snackBarHelper.ShowSuccessDarkSnackBar(_localization.CrewNameUpdatedSnackbarMessage);
            return true;
        }

        public async void UpdateCrewDescription(string description)
        {
            var isSuccess = await SaveChangesToCrewModel(description: description);
            if (!isSuccess) return;
            
            _snackBarHelper.ShowSuccessDarkSnackBar(_localization.CrewDescriptionUpdatedSnackbarMessage);
        }
        
        public async Task UpdateCrewCoverImages(List<FileInfo> filesInfo)
        {
            foreach (var file in filesInfo)
            {
                if (file.Source.UploadId is not {})
                {
                    FileInfo crewFile = _crewModel.Files.FirstOrDefault(crewFileInfo =>
                        crewFileInfo.FileType == file.FileType
                        && crewFileInfo.Resolution == file.Resolution);
                    file.Source.UploadId = crewFile?.Source.UploadId;
                }
            }

            await SaveChangesToCrewModel(files: filesInfo);
        }

        public async Task<bool> UpdateCrewLanguageAsync(long id)
        {
            var ok = await SaveChangesToCrewModel(languageId: id);

            if (ok) LanguageUpdated?.Invoke(id);

            return ok;
        }

        public void ChangeMemberRole(long userId, long roleId)
        {
            if (roleId == Constants.Crew.LEADER_ROLE_ID)
            {
                throw new ArgumentException("Use transfer ownership method instead");
            }

            if (userId == _localUser.GroupId)
            {
                ShowDemotionConfirmationPopup(() => RequestRoleChange(userId, roleId), null);
                return;
            }
            
            RequestRoleChange(userId, roleId);
        }

        public void TransferOwnership(long newLeaderId, Action onSuccess)
        {
            ShowDemotionConfirmationPopup(OwnershipTransferConfirmed, null);

            async void OwnershipTransferConfirmed()
            {
                var result = await _bridge.UpdateCrewMemberRole(_crewId, newLeaderId, Constants.Crew.LEADER_ROLE_ID);
                if (result.IsSuccess)
                {
                    var nickname = _crewModel.Members.First(m => m.Group.Id == newLeaderId).Group.Nickname;
                    var message = string.Format(_localization.TransferOwnershipSuccessSnackbarMessageFormat, nickname);

                    await RefreshCrewDataAsync(default);
                    _snackBarHelper.ShowSuccessDarkSnackBar(message);
                    onSuccess?.Invoke();
                }

                if (!result.IsError) return;
                if (result.ErrorMessage.Contains(MEMBER_NOT_FOUND_ERROR))
                {
                    _snackBarHelper.ShowFailSnackBar(_localization.MemberNotFoundSnackbarMessage);
                    await RefreshCrewDataAsync(default);
                    _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
                    _popupManager.ClosePopupByType(PopupType.TransferCrewOwnership);

                    return;
                }

                Debug.LogError(result.ErrorMessage);
            }
        }

        public async Task<bool> InviteUserToCrew(long groupId)
        {
            if (_crewModel.Members.Length == _crewModel.TotalMembersCount)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.CrewIsFullSnackbarMessage);
                return false;
            }

            var result = await _bridge.InviteUsersToCrew(_crewId, new[] { groupId });

            if (result.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.CrewIsFullSnackbarMessage);
                return false;
            }

            UserInvited?.Invoke(groupId);
            return true;
        }

        public async void AcceptRequest(long groupId, string nickname, long requestId)
        {
            if (Model.MembersCount == Model.TotalMembersCount)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.CrewIsFullSnackbarMessage);
                return;
            }

            var result = await _bridge.AcceptJoinCrewRequest(_crewId, groupId);

            if (result.IsSuccess)
            {
                var message = string.Format(_localization.UserRequestAcceptedSnackbarMessageFormat, nickname);
                _snackBarHelper.ShowSuccessDarkSnackBar(message);

                await RefreshCrewDataAsync(default);
                
                PlayerPrefs.SetInt(string.Format(Constants.Crew.JOIN_REQUEST_HANDLED_KEY, requestId), 1);
                
                return;
            }

            if (result.IsError)
            {
                if (result.ErrorMessage.Contains("AnotherCrewMember"))
                {
                    _snackBarHelper.ShowInformationSnackBar(_localization.UserIsInAnotherCrewSnackbarMessageFormat);
                    RejectRequest(groupId, nickname, requestId);

                    return;
                }

                Debug.LogError(result.ErrorMessage);
            }
        }

        public async void RejectRequest(long groupId, string nickname, long requestId, bool showSnackbar = true)
        {
            var result = await _bridge.IgnoreJoinCrewRequest(_crewId, groupId);
            if (result.IsSuccess && showSnackbar)
            {
                var message = string.Format(_localization.UserRequestRejectedSnackbarMessageFormat, nickname);
                _snackBarHelper.ShowSuccessDarkSnackBar(message);
                
                PlayerPrefs.SetInt(string.Format(Constants.Crew.JOIN_REQUEST_HANDLED_KEY, requestId), -1);
                
                await RefreshCrewDataAsync(default);
                return;
            }
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);
        }
        
        public async Task<LanguageInfo[]> GetCrewLanguages(CancellationToken token)
        {
            if (_languages != null)
            {
                return _languages;
            }
            
            var result = await _bridge.GetAvailableLanguages(token);
            if (result.IsSuccess)
            {
                _languages = result.Models;
                return result.Models;
            }
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);
            
            return null;
        }

        public void EnableChatInput(bool isEnabled)
        {
            if (isEnabled) SidebarDisabled?.Invoke();
            else SidebarActivated?.Invoke();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowSideBar(CrewSidebarPopupConfiguration config)
        {
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.CrewSidebar);
            SidebarActivated?.Invoke();
        }

        private async Task<bool> SaveChangesToCrewModel(string name = null, string description = null, bool? isPublic = null, 
            List<FileInfo> files = null, long? languageId = null)
        {
            var model = new SaveCrewModel
            {
                Name = name ?? _crewModel.Name,
                Description = description ?? _crewModel.Description,
                IsPublic = isPublic ?? _crewModel.IsPublic,
                MessageOfDay = _crewModel.MessageOfDay,
                Files = files ?? _crewModel.Files,
                LanguageId = languageId ?? _crewModel.LanguageId,
            };

            var result = await _bridge.UpdateCrewData(_crewId, model);
            if (result.IsSuccess)
            {
                _crewModel = result.Model;
                RefreshCachedData();
                CrewModelUpdated?.Invoke(result.Model);
                
                return true;
            }

            if (result.IsError)
            {
                if (result.ErrorMessage.Contains(NAME_TAKEN_ERROR))
                {
                    _snackBarHelper.ShowFailSnackBar(_localization.CrewNameIsAlreadyTakenSnackbarMessage);
                    return false;
                }

                _snackBarHelper.ShowFailSnackBar(_localization.EditAccessRestrictedSnackbarMessage);
                await RefreshCrewDataAsync(default);
            }

            _popupManager.ClosePopupByType(PopupType.EditCrew);
            return false;
        }
        
        private void ShowCantDeleteCrewPopup()
        {
            var cfg = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.DeleteCrewIsFullPopupTitle,
                Description = _localization.DeleteCrewIsFullPopupDescription,
                OnYes = HideDeleteCrewPopup,
                YesButtonText = _localization.DeleteCrewIsFullPopupConfirmButton
            };
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType, true);

            void HideDeleteCrewPopup()
            {
                _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
            }
        }

        private async void RequestRoleChange(long userId, long roleId)
        {
            var result = await _bridge.UpdateCrewMemberRole(_crewId, userId, roleId);
            if (result.IsSuccess)
            {
                _crewModel.Members.First(m => m.Group.Id == userId).RoleId = roleId;
                MembersListUpdated?.Invoke(_crewModel.Members);   
                _snackBarHelper.ShowInformationSnackBar(_localization.RoleUpdatedSnackbarMessage);
                await RefreshCrewDataAsync(default);

                if (userId == _localUser.GroupId && roleId > Constants.Crew.COORDINATOR_ROLE_ID)
                {
                    _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
                }
                return;
            }

            if (!result.IsError) return;
            if (result.ErrorMessage.Contains(MEMBER_NOT_FOUND_ERROR))
            {
                _snackBarHelper.ShowFailSnackBar(_localization.MemberNotFoundSnackbarMessage);
            }
            else if(result.ErrorMessage.Contains(INSUFFICIENT_ACCESS_LEVEL_ERROR))
            {
                _snackBarHelper.ShowFailSnackBar(_localization.EditAccessRestrictedSnackbarMessage);
            }

            await RefreshCrewDataAsync(default);
            _popupManager.ClosePopupByType(PopupType.ManageCrewMember);
        }

        private void ShowDemotionConfirmationPopup(Action onConfirmed, Action onCanceled)
        {
            var cfg = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.DemoteYourselfPopupTitle,
                Description = _localization.DemoteYourselfPopupDescription,
                YesButtonText = _localization.DemoteYourselfPopupConfirmButton,
                YesButtonSetTextColorRed = false,
                OnYes = Confirmed,
                NoButtonText = _localization.DemoteYourselfPopupCancelButton,
                OnNo = Canceled
            };
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType, true);

            void Confirmed()
            {
                _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
                onConfirmed?.Invoke();
            }
            void Canceled()
            {
                _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
                onCanceled?.Invoke();
            }
        }

        private void RefreshCachedData()
        {
            var localUserId = _localUser.GroupId;
            
            LocalUserMemberData = _crewModel.Members.First(m => m.Group.Id == localUserId);
            LocalUserIsAdmin = LocalUserMemberData.RoleId <= Constants.Crew.COORDINATOR_ROLE_ID;
            LocalUserIsLeader = LocalUserMemberData.RoleId == Constants.Crew.LEADER_ROLE_ID;
        }

        private void OnSidebarSlideOut()
        {
            SidebarDisabled?.Invoke();
        }

        public Task<Result> MuteCrewChatNotifications(MuteChatTimeOptions option)
        {
            return _bridge.MuteChatNotifications(_crewModel.ChatId, option);
        }
        
        private async Task WaitForProfile()
        {
            if (!_localUser.IsLoadingProfile)
            {
                await _localUser.DownloadProfile();
            }
            else
            {
                while (_localUser.IsLoadingProfile)
                {
                    await Task.Delay(25);
                }
            }
        }
    }
}