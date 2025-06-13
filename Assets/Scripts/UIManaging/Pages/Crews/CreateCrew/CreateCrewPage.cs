using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.Crew;
using Modules.InputHandling;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UIManaging.Pages.UserSelection;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    public class CreateCrewPage : GenericPage<CrewCreatePageArgs>
    {
        private const int MIN_CREW_NAME_LENGTH = 2;

        [SerializeField] private GameObject[] _creationStepPanels;
        [Space] 
        [SerializeField] private PageHeaderView _pageHeader;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _descriptionText;
        [Space]
        [SerializeField] private AdvancedInputField _crewNameInput;
        [SerializeField] private AdvancedInputField _crewDescriptionInput;
        [Space]
        [SerializeField] private ToggleGroup _privacyToggleGroup;
        [SerializeField] private Toggle _privateCrewToggle;
        [SerializeField] private Toggle _publicCrewToggle;
        [Space]
        [SerializeField] private FileUploadWidget _fileUpload;
        [SerializeField] private RawImage _uploadedFilePreview;
        [SerializeField] private AnimatedSkeletonBehaviour _photoUploadSkeleton;
        [SerializeField] private AspectRatioFitter _previewRatioFitter;
        [Space]
        [SerializeField] private UserSelectionWidget _invitedUsersSelectionWidget;
        [SerializeField] private GameObject _selectedUsersPanel;
        [Space]
        [SerializeField] private LanguageCloudView _languageCloud;

        [SerializeField] private Button _nextButton;
        [SerializeField] private GameObject _nextButtonParent;
        [SerializeField] private TMP_Text _nextButtonText;
        [SerializeField] private GameObject _nextButtonLoadingIndicator;
        [Space]
        [SerializeField] private TMP_Text _errorText;

        [Inject] private IBridge _bridge;
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private CrewService _crewService;
        [Inject] private SnackBarHelper _snackBarHelper;

        private CancellationTokenSource _tokenSource;
        private int _currentStep;
        private long _selectedLanguage = -1;

        private List<CreateCrewStep> _registrationSteps;
        private UserSelectionPanelModel _selectionPanelModel;

        public override PageId Id => PageId.CrewCreate;

        private CreateCrewStep CurrentStep => _registrationSteps[_currentStep];
        private int LastStepIndex => _registrationSteps.Count - 1;
        
        private void Start()
        {
            _errorText.SetActive(false);
            _selectionPanelModel = new UserSelectionPanelModel(100,
                                                                      null,
                                                                      null,
                                                                      null,
                                                                      UserSelectionPageArgs.UsersFilter.Friends);

            _selectionPanelModel.ItemSelectionChanged += OnUserSelectionChanged;
            _invitedUsersSelectionWidget.Initialize(_selectionPanelModel);
        }

        private void OnDisable()
        {
            if (_tokenSource == null) return;
            
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
        }
        

        private void OnNameInputChanged(string crewName)
        {
            _nextButton.interactable = crewName.Length >= MIN_CREW_NAME_LENGTH;
        }

        protected override void OnInit(PageManager pageManager)
        {
            _pageHeader.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, OnBackButton)));
            
            _fileUpload.MediaConversionStarted += EnableLoading;
            _fileUpload.OnPhotoConversionSuccess += OnPhotoConverted;
            _fileUpload.OnMediaConversionError += OnConversionError;
            _fileUpload.MediaType = NativeGallery.MediaType.Image;


            SetupToggleGroup();

            _nextButton.onClick.AddListener(OnNextButton);

            SetupSteps();
            
            _backButtonEventHandler.AddButton(gameObject,OnBackButton);
        }

        private void OnUserSelectionChanged(UserSelectionItemModel _)
        {
            _selectedUsersPanel.SetActive(_selectionPanelModel.SelectedItems.Count != 0);
        }

        private async void OnNextButton()
        {
            if (CurrentStep.Validation != null)
            {
                EnableLoading();
                var isInputValid = await CurrentStep.Validation.Invoke();
                DisableLoading();
                if (!isInputValid) return;
            }

            CurrentStep.OnClose?.Invoke();
            CurrentStep.OnValidationSuccess?.Invoke();
            
            _currentStep++;

            if (_currentStep > LastStepIndex)
            {
                CreateCrew();
                return;
            }

            EnableCurrentStepPanel();
        }

        private void EnableCurrentStepPanel()
        {
            for (var i = 0; i <= LastStepIndex; i++)
            {
                _creationStepPanels[i].SetActive(i == _currentStep);
            }
            
            _errorText.SetActive(false);
            SetHeaderStepCounter();
            DisableLoading();
            CurrentStep.OnStart?.Invoke();
            
            _nextButtonText.text = _currentStep == LastStepIndex 
                ? "Create crew"
                : "Continue";
        }

        private void OnBackButton()
        {
            if (_currentStep == 0)
            {
                Manager.MoveBack();
                return;
            }

            CurrentStep.OnClose?.Invoke();
            --_currentStep;
            EnableCurrentStepPanel();
        }

        private void SetHeaderStepCounter()
        {
            _pageHeader.Header = $"{_currentStep + 1} of {LastStepIndex + 1}";
            _headerText.text = CurrentStep.Header;

            if (string.IsNullOrEmpty(CurrentStep.Description))
            {
                _descriptionText.gameObject.SetActive(false);
            }
            else
            {
                _descriptionText.gameObject.SetActive(true);
                _descriptionText.text = CurrentStep.Description;
            }
        }

        private void OnConversionError(string obj)
        {
            SetError("That photo is inappropriate");
            DisableLoading();
            _nextButton.interactable = !OpenPageArgs.SaveCrewModel.Files.IsNullOrEmpty();
        }

        private void OnPhotoConverted(PhotoFullInfo obj)
        {
            var texture = _uploadedFilePreview.texture;
            var ratio = (float)texture.width / texture.height;
            _previewRatioFitter.aspectRatio = ratio;
            
            DisableLoading();
            SetError(null);
            
            var fileInfo = obj.Files.FirstOrDefault();
            if (fileInfo == null) return;
            OpenPageArgs.SaveCrewModel.Files = obj.Files;
            
        }

        private void EnableLoading()
        {
            _nextButton.interactable = false;
            _backButton.interactable = false;
            _backButtonEventHandler.ProcessEvents( false);
            _nextButtonLoadingIndicator.SetActive(true);
            _nextButtonText.SetActive(false);
            _photoUploadSkeleton.SetActive(true);
            _photoUploadSkeleton.Play();
        }

        private void DisableLoading()
        {
            _nextButton.interactable = true;
            _backButton.interactable = true;
            _backButtonEventHandler.ProcessEvents( true);
            _nextButtonLoadingIndicator.SetActive(false);
            _nextButtonText.SetActive(true);
            _photoUploadSkeleton.FadeOut();
        }

        private void SetError(string errorMessage)
        {
            _errorText.SetActive(true);
            _errorText.text = errorMessage ?? string.Empty;
        }

        protected override void OnDisplayStart(CrewCreatePageArgs args)
        {
            base.OnDisplayStart(args);

            _currentStep = 0;

            EnableCurrentStepPanel();
        }

        private void SetupToggleGroup()
        {
            _privacyToggleGroup.allowSwitchOff = false;
            _privateCrewToggle.group = _privacyToggleGroup;
            _publicCrewToggle.group = _privacyToggleGroup;
            _publicCrewToggle.isOn = true;
            _privateCrewToggle.isOn = false;
        }

        private void SetupSteps()
        {
            _registrationSteps = new List<CreateCrewStep>
            {
                new CreateCrewStep
                {
                    Header = "Choose a name",
                    OnStart = () =>
                    {
                        OnNameInputChanged(_crewNameInput.Text);
                        _crewNameInput.OnValueChanged.AddListener(OnNameInputChanged);
                        _crewNameInput.Select();
                    },
                    Validation = async () =>
                    {
                        if (_crewNameInput.Text.Length < MIN_CREW_NAME_LENGTH)
                        {
                            SetError($"Name should have at least {MIN_CREW_NAME_LENGTH} characters");
                            return false;
                        }

                        if (!await ModerateText(_crewNameInput.Text))
                        {
                            return false;
                        }
                        
                        var result = await _bridge.ValidateCrewName(_crewNameInput.Text);

                        if (result.IsSuccess)
                        {
                            if (result.Model.IsValid)
                            {
                                SetError(null);
                                return true;
                            }

                            SetError("This name is already taken");
                            return false;
                        }

                        SetError(result.ErrorMessage);
                        return false;
                    },
                    OnClose = () =>
                    {
                        _crewNameInput.OnValueChanged.RemoveListener(OnNameInputChanged);
                    },
                    OnValidationSuccess = () =>
                    {
                        OpenPageArgs.SaveCrewModel.Name = _crewNameInput.Text;
                    }
                },
                new CreateCrewStep
                {
                    Header = "Description",
                    Validation = () =>
                    {
                        if (_crewDescriptionInput.Text.Length == 0)
                        {
                            return Task.FromResult(true);
                        }
                        
                        return ModerateText(_crewDescriptionInput.Text);
                    },
                    OnValidationSuccess = () =>
                    {
                        OpenPageArgs.SaveCrewModel.Description = _crewDescriptionInput.Text;
                    }
                },
                new CreateCrewStep()
                {
                    Header = "Language",   
                    Description = "What language will you be using in the chat/crew",
                    OnStart = async () =>
                    {
                        _tokenSource = new CancellationTokenSource();
            
                        _nextButton.interactable = false;
                        var languages = await _crewService.GetCrewLanguages(_tokenSource.Token);
                        if (languages is null) return;

                        _languageCloud.Initialize(new LanguageCloudModel(languages));
                        _languageCloud.LanguageSelected += OnLanguageSelected;
                        
                        if (_selectedLanguage != -1) _languageCloud.SetLanguage(_selectedLanguage);
                    },
                    Validation = () =>
                    {
                        if (_selectedLanguage == -1) SetError("Select crew language to continue.");
                        
                        return Task.FromResult(_selectedLanguage != -1);
                    },
                    OnValidationSuccess = () => OpenPageArgs.SaveCrewModel.LanguageId = _selectedLanguage,
                },
                new CreateCrewStep
                {
                    Header = "Privacy",
                    OnValidationSuccess = () =>
                    {
                        OpenPageArgs.SaveCrewModel.IsPublic = _publicCrewToggle.isOn;
                    }
                },
                new CreateCrewStep
                {
                    Header = "Crew photo",
                    OnStart = () =>
                    {
                        _nextButton.interactable = !OpenPageArgs.SaveCrewModel.Files.IsNullOrEmpty();
                    },
                    Validation = () =>
                    {
                        var result = !OpenPageArgs.SaveCrewModel.Files.IsNullOrEmpty();

                        if (!result)
                        {
                            SetError("Upload your crew photo to continue");
                        }
                        
                        _nextButton.interactable = result;
                        
                        return Task.FromResult(result);
                    },
                },
                new CreateCrewStep
                {
                    Header = "Invite friends",
                }
            };
        }

        private async Task<bool> ModerateText(string text)
        {
            var result = await _bridge.ModerateTextContent(text);

            if (result.IsSuccess && result.PassedModeration) return true;
            
            SetError("Text contains inappropriate content");
            return false;
        }

        private async void CreateCrew()
        {
            EnableLoading();

            var result = await _bridge.CreateCrew(OpenPageArgs.SaveCrewModel);

            if (!result.IsSuccess)
            {
                DisableLoading();
                
                var error = JsonUtility.FromJson<ErrorResponse>(result.ErrorMessage);
                _snackBarHelper.ShowFailSnackBar(error.Message);
                Manager.MoveNext(new HomePageArgs(), false);
                return;
            }

            if (_invitedUsersSelectionWidget.SelectedUsers.Count > 0)
            {
                await _bridge.InviteUsersToCrew(result.Model.Id,
                                             _invitedUsersSelectionWidget.SelectedUsers
                                                .Select(user => user.Id).ToArray());
            }
            
            Manager.MoveNext(new CrewPageArgs(), false);
        }
        
        private void OnLanguageSelected(long id)
        {
            _selectedLanguage = id;
            _nextButton.interactable = id != 0;
        }

        [UsedImplicitly]
        class ErrorResponse
        {
            public string ErrorCode;
            public string Message;
            public string RequestId;
        }

    }
}
