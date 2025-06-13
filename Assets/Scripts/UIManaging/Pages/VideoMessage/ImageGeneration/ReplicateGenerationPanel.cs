using System.Collections;
using System.Collections.Generic;
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
    public class ReplicateGenerationPanel : BaseImageGenerationPanel
    {
        private const string NAME = "Fluffy Animal v2";
        private const string MODEL_VERSION = "4b8e37b91a60fdc9dba0e17a73267b5428afc232a9d2e33e67895fe521514536";

        private static readonly BackgroundOption[] OPTIONS_1 = new BackgroundOption[]
        {
            new BackgroundOption() { DisplayValue = "🐇", PromptValue = "Rabbit"},
            new BackgroundOption() { DisplayValue = "🐵", PromptValue = "Monkey"},
            new BackgroundOption() { DisplayValue = "🐄", PromptValue = "Cow"},
            new BackgroundOption() { DisplayValue = "🦔", PromptValue = "Hedgehog"},
            new BackgroundOption() { DisplayValue = "🐹", PromptValue = "Hamster"},

            new BackgroundOption() { DisplayValue = "🦉", PromptValue =  "Owl"},
            new BackgroundOption() { DisplayValue = "🐈", PromptValue =  "Cat"},
            new BackgroundOption() { DisplayValue = "🐎", PromptValue =  "Horse"},
            new BackgroundOption() { DisplayValue = "🦡", PromptValue =  "Badger"},
            new BackgroundOption() { DisplayValue = "🦜", PromptValue =  "Parrot"},

            new BackgroundOption() { DisplayValue = "🐻", PromptValue = "Bear"},
            new BackgroundOption() { DisplayValue = "🐯", PromptValue = "Tiger"},
            new BackgroundOption() { DisplayValue = "🦙", PromptValue = "Llama"},
            new BackgroundOption() { DisplayValue = "🦆", PromptValue = "Duck"},
            new BackgroundOption() { DisplayValue = "🐼", PromptValue = "Panda"},

            new BackgroundOption() { DisplayValue = "🦄", PromptValue = "Unicorn"},
            new BackgroundOption() { DisplayValue = "🐁", PromptValue = "Mouse"},
            new BackgroundOption() { DisplayValue = "🐺", PromptValue = "Wolf"},
            new BackgroundOption() { DisplayValue = "🐘", PromptValue = "Elephant"},
            new BackgroundOption() { DisplayValue = "🐧", PromptValue = "Penguin"},

            new BackgroundOption() { DisplayValue = "🦭", PromptValue = "Seal"},
            new BackgroundOption() { DisplayValue = "🦊", PromptValue = "Fox"},
            new BackgroundOption() { DisplayValue = "🐕", PromptValue = "Dog"},
            new BackgroundOption() { DisplayValue = "🦒", PromptValue = "Giraffe"},
            new BackgroundOption() { DisplayValue = "🦁", PromptValue = "Lion"},
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

        private static readonly BackgroundOption[] OPTIONS_3 = new BackgroundOption[]
        {
            new BackgroundOption() { DisplayValue = "🏖", PromptValue = "a Beach"},
            new BackgroundOption() { DisplayValue = "🏙", PromptValue = "the City"},
            new BackgroundOption() { DisplayValue = "⛰", PromptValue = "a Mountain"},

            new BackgroundOption() { DisplayValue = "🏫", PromptValue = "a School"},
            new BackgroundOption() { DisplayValue = "🍝", PromptValue = "a Restaurant"},
            new BackgroundOption() { DisplayValue = "🛏", PromptValue = "a Bedroom"},

            new BackgroundOption() { DisplayValue = "🎭", PromptValue = "a Stage"},
            new BackgroundOption() { DisplayValue = "🏰", PromptValue = "a Castle"},
            new BackgroundOption() { DisplayValue = "💃", PromptValue = "a Ballroom"},
        };

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OpenGenerationPopup()
        {
            var optionSet1 = new BackgroundOptionsSet
            {
                Title = "Pick an animal",
                ColumnsCount = 5,
                Options = OPTIONS_1,
            };

            var optionSet2 = new BackgroundOptionsSet
            {
                Title = "Pick a color",
                ColumnsCount = 3,
                Options = OPTIONS_2,
            };

            var optionSet3 = new BackgroundOptionsSet
            {
                Title = "Pick a place",
                ColumnsCount = 3,
                Options = OPTIONS_3,
            };

            var optionSets = new [] {optionSet1, optionSet2, optionSet3};
            var configuration = new AiGridPopupConfiguration(Logo, optionSets, OnConfirm);

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.AiGridPopup);

            void OnConfirm(string[] selectedValues)
            {
                var animal = selectedValues[0];
                var color = selectedValues[1];
                var place = selectedValues[2];

                var prompt = $"In the style of TOK, generate a background image where one small cute Pixar like full body " +
                             "fluffy fantasy animal is positioned in the bottom right corner. The animal should have some " +
                             $"traits from a {color} {animal}, and the image is for someone who likes {place}.";

                var negativePrompt = "The image should not contain any characters except for the animal, no text and no borders.";

                GenerateImage(prompt, negativePrompt, new[] {animal, color, place});
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void GenerateImage(string prompt, string negativePrompt, IEnumerable selectedOptions)
        {
            LoadingOverlay.Show(CancelImageGeneration);

            var imageRequest = new ReplicateRequest()
            {
                Version = MODEL_VERSION,
                Input = new ReplicateInput()
                {
                    Width = 576,
                    Height = 1024,
                    Prompt = prompt,
                    Negative_prompt = negativePrompt,
                    Num_outputs = 1,
                    Lora_scale = 0.8f,
                    Guidance_scale = 7.5f,
                    High_noise_frac = 0.8f,
                    Num_inference_steps = 25,
                }
            };

            var token = CreateNewTokenSource().Token;
            var response = await _bridge.GenerateImage(imageRequest, token);
            if (response.IsRequestCanceled) return;

            if (response.IsError)
            {
                Debug.LogError($"Failed to generate image. Reason: {response.ErrorMessage}");
            }
            else
            {
                ApplyImageAsBackground(response.Model.UploadId, response.Model.LocalFilePath);
                LogAmplitudeEvent(selectedOptions, prompt, response.Model.UploadId);
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

        private void LogAmplitudeEvent(IEnumerable selectedOptions, string prompt, string uploadId)
        {
            var props = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.NAME] = NAME,
                [AmplitudeEventConstants.EventProperties.OPTIONS_SELECTED] = selectedOptions,
                [AmplitudeEventConstants.EventProperties.PROMPT] = prompt,
                [AmplitudeEventConstants.EventProperties.UPLOAD_ID] = uploadId,
            };

            _amplitudeManager.LogEventWithEventProperties(VME_BACKGROUND_GENERATED, props);
        }
    }
}