using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.ClientServer.ImageGeneration;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Extensions;
using Modules.Amplitude;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LocalStorage;
using UIManaging.Pages.VideoMessage.ImageGeneration;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.AiGeneration;
using UnityEngine;
using Zenject;
using static Modules.Amplitude.AmplitudeEventConstants.EventNames;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class BackgroundGenerationPanel : MonoBehaviour
    {
        [SerializeField] private GenerationProgressOverlay _loadingOverlay;

        [Inject] private IBridge _bridge;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private ILevelManager _levelManager;

        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // public
        //---------------------------------------------------------------------

        public void OpenGenerationPopup(SetLocationBackgroundSettings backgroundEntity)
        {
            var configuration = new AiGridPopupConfiguration(backgroundEntity, OnConfirm);

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.AiGridPopup);

            void OnConfirm(string[] selectedValues)
            {
                GenerateImage(backgroundEntity, selectedValues);
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            CancelCurrentRequest();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void GenerateImage(SetLocationBackgroundSettings backgroundEntity,
            string[] selectedOptions)
        {
            var settings = backgroundEntity.Settings;

            var prompts = settings.Prompts.OrderByDescending(x => x.Weight).ToArray();

            // ReSharper disable CoVariantArrayConversion
            var positivePrompt = prompts.FirstOrDefault()?.Text ?? string.Empty;
            positivePrompt = string.Format(positivePrompt, selectedOptions);

            var negativePrompt = prompts.LastOrDefault()?.Text ?? string.Empty;
            negativePrompt = string.Format(negativePrompt, selectedOptions);
            // ReSharper restore CoVariantArrayConversion

            var imageRequest = new ReplicateRequest()
            {
                Version = settings.ModelVersion,
                Input = new ReplicateInput()
                {
                    Width = settings.Width,
                    Height = settings.Height,
                    Prompt = positivePrompt,
                    Negative_prompt = negativePrompt,
                    Lora_scale = settings.LoraScale,
                    Guidance_scale = settings.GuidanceScale,
                    Num_inference_steps = settings.DiffusionSteps,
                }
            };

            _loadingOverlay.Show(CancelImageGeneration);

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

                var name = backgroundEntity.Name;
                var model = settings.ModelVersion;
                var uploadId = response.Model.UploadId;
                LogAmplitudeEvent(name, model, selectedOptions, positivePrompt, uploadId);
            }

            _loadingOverlay.Hide();
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

        //---------------------------------------------------------------------
        // Token
        //---------------------------------------------------------------------

        private CancellationTokenSource CreateNewTokenSource()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            return _tokenSource;
        }

        private void CancelImageGeneration()
        {
            CancelCurrentRequest();
            _loadingOverlay.Hide();
        }

        private void CancelCurrentRequest()
        {
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
        }

        //---------------------------------------------------------------------
        // Amplitude
        //---------------------------------------------------------------------

        private void LogAmplitudeEvent(string backgroundName, string model, IEnumerable<string> selectedOptions, string prompt, string uploadId)
        {
            var props = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.NAME] = backgroundName,
                [AmplitudeEventConstants.EventProperties.MODEL_VERSION] = model,
                [AmplitudeEventConstants.EventProperties.OPTIONS_SELECTED] = selectedOptions,
                [AmplitudeEventConstants.EventProperties.PROMPT] = prompt,
                [AmplitudeEventConstants.EventProperties.UPLOAD_ID] = uploadId,
            };

            _amplitudeManager.LogEventWithEventProperties(VME_BACKGROUND_GENERATED, props);
        }
    }
}