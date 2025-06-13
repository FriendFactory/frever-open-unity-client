using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.ClientServer.ImageGeneration;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Modules.Amplitude;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LocalStorage;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.AiGeneration;
using UnityEngine;
using Zenject;
using static Modules.Amplitude.AmplitudeEventConstants.EventNames;

namespace UIManaging.Pages.VideoMessage.ImageGeneration
{
    public class StabilityGenerationPanel : BaseImageGenerationPanel
    {
        private const string NAME = "Zodiac Sign";

        private static readonly BackgroundOption[] OPTIONS_1 = new BackgroundOption[]
        {
            new BackgroundOption() { DisplayValue = "♈", PromptValue = "Aries", Label = "Aries"},
            new BackgroundOption() { DisplayValue = "♉", PromptValue = "Taurus", Label = "Taurus"},
            new BackgroundOption() { DisplayValue = "♊", PromptValue = "Gemini", Label = "Gemini"},
            new BackgroundOption() { DisplayValue = "♋", PromptValue = "Cancer", Label = "Cancer"},

            new BackgroundOption() { DisplayValue = "♌", PromptValue = "Leo", Label = "Leo"},
            new BackgroundOption() { DisplayValue = "♍", PromptValue = "Virgo", Label = "Virgo"},
            new BackgroundOption() { DisplayValue = "♎", PromptValue = "Libra", Label = "Libra"},
            new BackgroundOption() { DisplayValue = "♏", PromptValue = "Scorpio", Label = "Scorpio"},

            new BackgroundOption() { DisplayValue = "♐", PromptValue = "Sagittarius", Label = "Sagittarius"},
            new BackgroundOption() { DisplayValue = "♑", PromptValue = "Capricorn", Label = "Capricorn"},
            new BackgroundOption() { DisplayValue = "♒", PromptValue = "Aquarius", Label = "Aquarius"},
            new BackgroundOption() { DisplayValue = "♓", PromptValue = "Pisces", Label = "Pisces"},
        };

        private static readonly BackgroundOption[] OPTIONS_2 = new BackgroundOption[]
        {
            new BackgroundOption() { DisplayValue = "🔴", PromptValue = "Red"},
            new BackgroundOption() { DisplayValue = "🟠", PromptValue = "Orange"},
            new BackgroundOption() { DisplayValue = "🟡", PromptValue = "Yellow"},

            new BackgroundOption() { DisplayValue = "🟢", PromptValue = "Green"},
            new BackgroundOption() { DisplayValue = "🔵", PromptValue = "Blue"},
            new BackgroundOption() { DisplayValue = "🟣", PromptValue = "Purple"},

            new BackgroundOption() { DisplayValue = "🟤", PromptValue = "Brown"},
            new BackgroundOption() { DisplayValue = "⚪", PromptValue = "White"},
            new BackgroundOption() { DisplayValue = "⚫", PromptValue = "Black"},
        };

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override void OpenGenerationPopup()
        {
            var optionSet1 = new BackgroundOptionsSet
            {
                Title = "Pick a sign",
                ColumnsCount = 4,
                Options = OPTIONS_1,
            };

            var optionSet2 = new BackgroundOptionsSet
            {
                Title = "Pick a color",
                ColumnsCount = 3,
                Options = OPTIONS_2,
            };

            var optionSets = new [] {optionSet1, optionSet2};
            var configuration = new AiGridPopupConfiguration(Logo, optionSets, OnConfirm);

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.AiGridPopup);

            void OnConfirm(string[] selectedValues)
            {
                var sign = selectedValues[0];
                var color = selectedValues[1];

                var mainPrompt = new TextPrompt()
                {
                    Text = $"Generate a background image. The background image should be a {sign} poster. " +
                           $"The {sign} symbol should be featured in the center of the image as a symbol and " +
                           $"the image also be inspired by {color}.",

                    Weight = 1f
                };

                var negativePrompt1 = new TextPrompt()
                {
                    Text = $"Blurry or pixelated image.",
                    Weight = -1f
                };

                var negativePrompt2 = new TextPrompt()
                {
                    Text = $"Any text or people.",
                    Weight = -1f
                };

                GenerateImage(new List<TextPrompt>() {mainPrompt, negativePrompt1, negativePrompt2}, new[] {sign, color});
            }
        }

        private async void GenerateImage(List<TextPrompt> prompts, IEnumerable selectedOptions)
        {
            LoadingOverlay.Show(CancelImageGeneration);

            var imageRequest = new CreateImageRequest()
            {
                Engine = Engine.SDXL_v1_0,
                Width = 768,
                Height = 1344,
                TextPrompts = prompts,
                Steps = 25,
                CfgScale = 7
            };

            CreateNewTokenSource();
            var response = await _bridge.GenerateImage(imageRequest);
            if (TokenSource == null || TokenSource.IsCancellationRequested) return;

            if (response.IsError)
            {
                Debug.LogError($"Failed to generate image. Reason: {response.ErrorMessage}");
            }
            else
            {
                ApplyImageAsBackground(response.Model.UploadId, response.Model.LocalFilePath);
                LogAmplitudeEvent(selectedOptions, prompts, response.Model.UploadId);
            }

            LoadingOverlay.Hide();
        }

        private void ApplyImageAsBackground(string uploadId, string filePath)
        {
            var fileInfo = new FileInfo(filePath, FileType.MainFile, FileExtension.Png)
            {
                Source = new FileSource() { UploadId = uploadId }
            };

            var userPhoto = new PhotoFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(PhotoFullInfo)),
                Files = new List<FileInfo> {fileInfo}
            };

            _levelManager.ApplySetLocationBackground(userPhoto);
        }

        private void LogAmplitudeEvent(IEnumerable selectedOptions, List<TextPrompt> prompts, string uploadId)
        {
            var props = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.NAME] = NAME,
                [AmplitudeEventConstants.EventProperties.OPTIONS_SELECTED] = selectedOptions,
                [AmplitudeEventConstants.EventProperties.PROMPT] = prompts.FirstOrDefault().Text,
                [AmplitudeEventConstants.EventProperties.UPLOAD_ID] = uploadId,
            };

            _amplitudeManager.LogEventWithEventProperties(VME_BACKGROUND_GENERATED, props);
        }
    }
}