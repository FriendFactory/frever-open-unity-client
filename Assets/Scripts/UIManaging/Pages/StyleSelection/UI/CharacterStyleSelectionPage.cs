using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Implementation.Onboarding;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.StyleSelection.UI;
using UnityEngine;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.UmaEditorPage.Ui.Stages
{
    internal sealed class CharacterStyleSelectionPage : BaseStyleSelectionPage<CharacterStyleSelectionArgs>
    {
        private const string DEFAULT_GENDER_IDENTIFIER = "female";
        
        [SerializeField] private GameObject _loadingIndicator;
        [SerializeField] private List<BodyTypeButton> _buttons;

        [Inject] private CharacterManager _characterManager;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IBridge _bridge;

        private ThumbnailsHelper _thumbnailsHelper;
        private Gender _selectedGender;
        private CancellationTokenSource _loadingCancellationTokenSource;

        private readonly Dictionary<Gender, Dictionary<CharacterInfo, Sprite>> _thumbnailsByGender = new();
        private Race _lastCachedRace;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.CharacterStyleSelection;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _buttons.ForEach(b => b.ButtonClick += OnBodyTypeButtonSelected);

            var combinedList = _styleSelectionList.GetComponent<CharacterStyleSelectionList>();
            combinedList.OnSelfieElementSelected += OnSelfieElementSelected;
            combinedList.OnSelfieButtonClicked += OnSelfieButtonClicked;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyStyleThumbnails();
            _buttons.ForEach(b => b.ButtonClick -= OnBodyTypeButtonSelected);

            var combinedList = _styleSelectionList.GetComponent<CharacterStyleSelectionList>();
            combinedList.OnSelfieElementSelected -= OnSelfieElementSelected;
            combinedList.OnSelfieButtonClicked -= OnSelfieButtonClicked;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void OnDisplayStart(CharacterStyleSelectionArgs args)
        {
            ShowLoadingUI(true);

            _buttons.ForEach(button =>
            {
                button.SetInteractable(false);
                var gender = args.Race.Genders.First(g => string.Equals(button.GenderIdentifier, g.Identifier, StringComparison.InvariantCultureIgnoreCase));
                button.Init(gender);
                button.gameObject.SetActive(gender.CanCreateCharacter);
            });
            
            UpdateGenderButtonsRoundness();

            _thumbnailsHelper ??= new ThumbnailsHelper(_bridge);

            var needToCleanUpCache = !HasFullGenderMatch(args.Race.Genders);
            var needToUpdateCache = needToCleanUpCache || _lastCachedRace is null;
            if (needToCleanUpCache)
            {
                ClearStyleSelectionList();
                ForgetPreviouslySelectedStyle(args.Race);
            }

            args.SelectedGender ??=
                args.Race.Genders.FirstOrDefault(g => g.Identifier.ToLower().Contains(DEFAULT_GENDER_IDENTIFIER)) ??
                args.Race.Genders.FirstOrDefault();
            
            _buttons.ForEach(b => b.Refresh(args.SelectedGender));

            if (needToUpdateCache)
            {
                await UpdateGendersDictionary(args.Race);
                _lastCachedRace = args.Race;
            }

            FillStyles(_thumbnailsByGender[args.SelectedGender]);

            base.OnDisplayStart(args);

            _buttons.ForEach(b => b.SetInteractable(true));

            OnBodyTypeButtonSelected(args.SelectedGender);

            if (args.SelectedCreateMode == CreateMode.Selfie)
            {
                _styleSelectionList.GetComponent<CharacterStyleSelectionList>().SetSelectedSelfieItem();
            } 
            else
            {
                _confirmButton.interactable = true;
            }

            OpenPageArgs?.OnPageDispayed?.Invoke();
            
            _ = _localUser.UpdateBalance().ConfigureAwait(false);
            
            ShowLoadingUI(false);
        }

        protected override void OnBackButtonClicked()
        {
            CancelLoadingCancellationTokenIfExists();

            OpenPageArgs.SelectedGender = _selectedGender;
            
            OpenPageArgs.OnBackButtonClicked?.Invoke(_styleSelectionList.SelectedStyle, _selectedGender);
        }

        protected override void OnConfirmButtonClicked()
        {
            var selectedStyle = _styleSelectionList.SelectedStyle;
            OpenPageArgs.OnStyleSelected(selectedStyle, _selectedGender);
            base.OnConfirmButtonClicked();
        }

        protected override void FillStyles(IReadOnlyDictionary<CharacterInfo, Sprite> thumbnails)
        {
            if (thumbnails is null) return;

            var stylePresets = thumbnails.Keys;
            var cellSize = 850.0f;
            var containsSelfieButton = _metadataProvider.MetadataStartPack.IsSelfieToAvatarSupportedByRace(OpenPageArgs.Race.Id) &&
                                       DeviceInformationHelper.DeviceSupportsTrueDepth();
            var styleListModel = new StyleSelectionListModel(stylePresets.ToArray(), thumbnails, cellSize, containsSelfieButton);
            _styleSelectionList.Initialize(styleListModel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowLoadingUI(bool show)
        {
            _confirmButton.interactable = !show;
            _loadingIndicator.SetActive(show);
        }

        private async Task UpdateGendersDictionary(Race race)
        {
            var genders = race.Genders;
            DestroyStyleThumbnails();
            _thumbnailsByGender.Clear();

            var loadingTasks = genders.Select(async gender =>
            {
                var stylePresets = await GetStylePresets(gender);
                if (stylePresets.Count == 0)
                {
                    return (gender, null);
                }

                var thumbnails = await _thumbnailsHelper.LoadThumbnailsAsync(stylePresets);
                return (gender, thumbnails);
            }).ToList();

            var results = await Task.WhenAll(loadingTasks);

            foreach (var (gender, thumbnails) in results)
            {
                if (thumbnails != null && thumbnails.Count > 0)
                {
                    _thumbnailsByGender.Add(gender, thumbnails);
                }
            }
        }

        private void DestroyStyleThumbnails()
        {
            if (_thumbnailsByGender.Count <= 0) return;
            var sprites = _thumbnailsByGender.Values.SelectMany(x => x.Values);
            foreach (var sprite in sprites)
            {
                Destroy(sprite.texture);
            }
        }

        private async Task<ICollection<CharacterInfo>> GetStylePresets(Gender gender)
        {
            var stylePresets = new List<CharacterInfo>();
            var universe = _metadataProvider.MetadataStartPack.GetUniverseByGenderId(gender.Id);
            var styles = await _characterManager.GetCharacterStyles(universe.Id);
            if (styles == null) return stylePresets;
            
            var styleCharacters = styles.Where(preset => preset.GenderId == gender.Id);
            stylePresets.AddRange(styleCharacters);

            return stylePresets;
        }

        private void CancelLoadingCancellationTokenIfExists()
        {
            _loadingCancellationTokenSource?.Cancel();
            _loadingCancellationTokenSource = null;
        }

        private void OnBodyTypeButtonSelected(Gender gender)
        {
            _buttons.ForEach(b => b.Refresh(gender));
            _selectedGender = gender;
            FillStyles(_thumbnailsByGender[gender]);
        }

        private void ForgetPreviouslySelectedStyle(Race race)
        {
            OpenPageArgs.SelectedGender = null;
            OpenPageArgs.SelectedStyle = null;
            _selectedGender = null;
        }

        private bool HasFullGenderMatch(Gender[] genderList)
        {
            genderList = genderList.Where(g => g.CanCreateCharacter).ToArray();
            if (_thumbnailsByGender is null
                || genderList is null
                || _thumbnailsByGender.Count != genderList.Length)
                return false;
    
            return genderList.All(listGender => 
                _thumbnailsByGender.Keys.Any(dictGender => dictGender.Id == listGender.Id));
        }

        private void OnSelfieElementSelected(bool selected)
        {
            _confirmButton.interactable = !selected;
        }

        private void OnSelfieButtonClicked()
        {
            OpenPageArgs.OnSelfieButtonClicked?.Invoke(_selectedGender);
        }

        private void UpdateGenderButtonsRoundness()
        {
            if (_buttons is null || _buttons.Count <= 0)
            {
                return;
            }

            if (_buttons.Count == 1)
            {
                _buttons[0].SetPositionPreset(BodyTypeButtonPosition.TheOnly);
                return;
            }

            var left = _buttons.FirstOrDefault(b => b.isActiveAndEnabled);
            var right = _buttons.LastOrDefault(b => b.isActiveAndEnabled);
            left.SetPositionPreset(BodyTypeButtonPosition.Left);
            right.SetPositionPreset(BodyTypeButtonPosition.Right);

            var rest = _buttons.Where(b => b != left && b != right && b.isActiveAndEnabled).ToList();
            rest.ForEach(b => b.SetPositionPreset(BodyTypeButtonPosition.Middle));
        }
    }
}