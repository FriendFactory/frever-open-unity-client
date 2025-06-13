using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.VideoServer;
using Extensions;
using Models;
using Navigation.Args;
using TMPro;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class LevelVideoPostSettingsPanel : PostVideoSettingsPanelBase
    {
        private const long PUBLIC_GROUP = 1;
        
        [SerializeField] private PublishVideoContentAccessLevelBasedSettings _contentAccessSettings;
        [Header("Save to device")]
        [SerializeField] private VideoPostSettingsToggle _saveToDeviceToggle;
        [SerializeField] private VideoPostSettingsToggle _landscapeModeToggle;
        [SerializeField] private PublishPageToggle _ultraHDToggle;
        [Header("Tagged Members")]
        [SerializeField] private TextMeshProUGUI _taggedMembersCount;
        [SerializeField] private GameObject _taggedMembersCountSetting;
        [Header("Template")]
        [SerializeField] private VideoPostSettingsToggle _templateToggle;
        [SerializeField] private Button _editTemplateButton;
        [SerializeField] private CanvasGroup _editTemplateButtonCanvasGroup;
        [SerializeField] private TMP_Text _templateLabel;
        
        [Inject] private INativeGalleryPermissionsHelper _nativeGalleryPermissionsHelper;
        [Inject] private PopupManager _popupManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private Level _level;
        private TemplatePublishSettings _templatePublishSettings;
        private bool _isLandscapeMode;
        private string _templateName;

        public bool SaveToDevice { get; private set; }

        public bool IsLandscapeMode
        {
            get => !_level.IsVideoMessageBased() && SaveToDevice && _isLandscapeMode;
            private set => _isLandscapeMode = value;
        }
        public bool IsUltraHDMode { get; private set; }
        public bool GenerateTemplate { get; private set; }

        public string GenerateTemplateName
        {
            get => _templateName;
            private set
            {
                _templateName = value;
                VideoPostAttributesModel.TemplateName.Value = value;
            }
        }

        public override IPublishVideoContentAccessSettings ContentAccessSettings => _contentAccessSettings;
        
        private bool IsLandscapeModeToggleShouldBeVisible => _saveToDeviceToggle.IsOn && !_level.IsVideoMessageBased();
        private static bool IsUltraHDToggleVisible => false;
        private bool _initialized;
        
        public void Init(Level level, VideoUploadingSettings settings, GroupInfo originalCreator, TemplateInfo initialTemplate)
        {
            if (_initialized) return;
            _initialized = true;
            
            _level = level;
            var publishInfo = settings.PublishInfo;
            if (publishInfo != null)
            {
                IsUltraHDMode = settings.PublishInfo.IsUltraHDMode;
                GenerateTemplate = settings.PublishInfo.GenerateTemplate;
                GenerateTemplateName = settings.PublishInfo.GenerateTemplateWithName;
                ExternalLink = settings.PublishInfo.ExternalLink;
                ExternalLinkType = settings.PublishInfo.ExternalLinkType;
                Access = settings.PublishInfo.Access;
            }
            else
            {
                _ultraHDToggle.IsOn = IsUltraHDMode;
            }
            
            SaveToDevice = _saveToDeviceToggle.IsOn;
            IsLandscapeMode = _landscapeModeToggle.IsOn;
            _templatePublishSettings = new TemplatePublishSettings(level, settings);
            
            var taggedIds = GetTaggedMemberGroupIds();
            _taggedMembersCount.text = taggedIds.Length.ToString();
            _taggedMembersCountSetting.SetActive(!level.IsVideoMessageBased());

            _saveToDeviceToggle.AddListener(OnSaveToDeviceToggleChanged);
            _saveToDeviceToggle.AddToggleValidation(CheckGalleryPermission);

            _landscapeModeToggle.AddListener(OnLandscapeModeToggleChanged);

            _ultraHDToggle.AddListener(OnUltraHDToggleChanged);
            
            SetupEditTemplateButton();

            _landscapeModeToggle.SetActive(IsLandscapeModeToggleShouldBeVisible);
            OnLandscapeModeToggleChanged(_landscapeModeToggle.IsOn);

            _ultraHDToggle.SetActive(IsUltraHDToggleVisible);
            OnUltraHDToggleChanged(_ultraHDToggle.IsOn);
            
            InitializeContentAccessSettings(settings);
            
            _saveToDeviceToggle.Initialize();
            _landscapeModeToggle.Initialize();

            if (_templatePublishSettings.ShowTemplatePopup)
            {
                OpenEditTemplatePage();
                settings.PublishInfo.ShowTemplatePopup = false;
            }

            VideoPostAttributesModel.OriginalCreator.Value = originalCreator?.Nickname ?? string.Empty;
            VideoPostAttributesModel.TemplateName.Value = initialTemplate?.Title ?? string.Empty;

            VideoPostAttributesModel.RefreshTaggedGroupsInfo(level);
            
            base.Init();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private bool CheckGalleryPermission()
        {
            if (_nativeGalleryPermissionsHelper.IsPermissionGranted(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Video))
            {
                return true;
            }
            
            _nativeGalleryPermissionsHelper.RequestPermission(NativeGallery.PermissionType.Write,
                                                              () => _saveToDeviceToggle.IsOn = true,
                                                              () => _saveToDeviceToggle.IsOn = false);
            
            var isGranted = _nativeGalleryPermissionsHelper.IsPermissionGranted(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Video);
            return isGranted;
        }
        
        private void OnSaveToDeviceToggleChanged(bool value)
        {
            SaveToDevice = value;
            _landscapeModeToggle.SetActive(IsLandscapeModeToggleShouldBeVisible);
            _ultraHDToggle.SetActive(IsUltraHDToggleVisible);
        }
        
        private void OnLandscapeModeToggleChanged(bool value)
        {
            IsLandscapeMode = value;
            _ultraHDToggle.SetActive(IsUltraHDToggleVisible);
        }
        
        private void GenerateTemplateConfirmed(bool generateTemplate, string newName)
        {
            if (GenerateTemplateName != name && generateTemplate)
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.TemplateNameSavedMessage);
            }
            
            GenerateTemplate = generateTemplate;
            GenerateTemplateName = newName;
            RefreshTemplateNameLabel();
            UpdateTemplateToggle();
            if (generateTemplate)
            { 
                Access = VideoAccess.Public;
            }
        }

        private void OnUltraHDToggleChanged(bool value)
        {
            IsUltraHDMode = value;
        }
        
        protected override void OnPrivacyValueChanged(VideoAccess videoAccess)
        {
            base.OnPrivacyValueChanged(videoAccess);
            
            if (videoAccess != VideoAccess.Public && GenerateTemplate)
            {
                ShowPublicAccessRequiredForPrivacyTogglePopup(videoAccess);
            }
        }

        protected override long[] GetTaggedMemberGroupIds()
        {
            var groups = _level.Event
                              .SelectMany(ev => ev.CharacterController)
                              .Select(controller => controller.Character.GroupId)
                              .Where(id => id != PUBLIC_GROUP && id != Bridge.Profile.GroupId)
                              .Distinct()
                              .ToArray();
            return groups;
        }

        private void ShowPublicAccessRequiredForPrivacyTogglePopup(VideoAccess value)
        {
            var config = new DialogDarkPopupConfiguration()
            {
                Title = _localization.TemplateVideoPrivacyPopupTitle,
                Description = _localization.TemplateVideoPrivacyPopupText,
                YesButtonText = _localization.TemplateVideoPrivacyPopupYesText,
                NoButtonText = _localization.TemplateVideoPrivacyPopupNoText,
                OnYes = () => { GenerateTemplate = false; },
                OnNo = () => Access = VideoAccess.Public,
                PopupType = PopupType.DialogDark
            };

            _popupManager.PushPopupToQueue(config);
        }

        private void SetupEditTemplateButton()
        {
            _templateToggle.Initialize();
            _templateToggle.IsOn = GenerateTemplate;
            _templateToggle.AddListener(OnTemplateToggleChanged);
            _editTemplateButton.onClick.AddListener(OpenEditTemplatePage);
                                    
            _editTemplateButtonCanvasGroup.SetActive(_templatePublishSettings.IsVideoEligibleForTemplate);
            RefreshTemplateNameLabel();
        }

        private async void OnTemplateToggleChanged(bool value)
        {
            if (value)
            {
                await Task.Delay(150);
                OpenEditTemplatePage();
            }
            else
            {
                GenerateTemplateConfirmed(false, GenerateTemplateName);
            }
        }

        private void RefreshTemplateNameLabel()
        {
            _templateLabel.text = GenerateTemplate ? GenerateTemplateName : string.Empty;
        }

        private void OpenEditTemplatePage()
        {
            if (!_templatePublishSettings.IsVideoEligibleForTemplate)
            {
                _snackBarHelper.ShowInformationSnackBar(GetTemplateGenerationUnavailableReason());
                return;
            }

            var config = new EditTemplatePopupConfiguration(GenerateTemplateName, 
                                                            GenerateTemplateConfirmed, 
                                                            OnEditTemplateBackButton);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnEditTemplateBackButton()
        {
            GenerateTemplate = false;
            UpdateTemplateToggle();
        }
        
        private void UpdateTemplateToggle() 
        {
            _templateToggle.RemoveListener(OnTemplateToggleChanged);
            _templateToggle.IsOn = GenerateTemplate;
            _templateToggle.AddListener(OnTemplateToggleChanged);
        }

        private string GetTemplateGenerationUnavailableReason()
        {
            if (_level.UsesTemplateEvent())
            {
                return _localization.TemplateLockedReasonTemplate;
            }

            if (_level.IsRemix())
            {
                return _localization.TemplateLockedReasonRemix;
            }

            return string.Empty;
        }

        private void InitializeContentAccessSettings(VideoUploadingSettings settings)
        {
            var isVideoMessageBased = _level.IsVideoMessageBased();
            var forceRemix = _level.IsRemix() || isVideoMessageBased;
            var allowRemix = !isVideoMessageBased;
            
            _contentAccessSettings.Initialize(new ContentAccessLevelBasedSettingsModel(forceRemix, allowRemix, settings));
        }
    }
}