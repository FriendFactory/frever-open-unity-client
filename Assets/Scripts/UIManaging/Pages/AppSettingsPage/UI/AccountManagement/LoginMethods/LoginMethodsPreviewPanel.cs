using System;
using System.Collections.Generic;
using Bridge.AccountVerification.Models;
using Common.Abstract;
using Common.Collections;
using Extensions;
using Modules.AccountVerification.LoginMethods;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    internal sealed class LoginMethodsPreviewPanel: BaseContextPanel<LoginMethodsPreviewPanelModel>
    {
        [SerializeField] private LoginMethodsDictionary _loginMethods;

        [Inject] private LoginMethodsLocalization _localization;

        public event Action<ILoginMethodInfo> NextButtonClicked;
        
        protected override void OnInitialized()
        {
            var methodPreviewModels = ContextData.GetModels();

            SetupPreviews(methodPreviewModels);

            ContextData.LoginMethodsUpdated += OnLoginMethodsUpdated;
        }

        protected override void BeforeCleanUp()
        {
            ContextData.LoginMethodsUpdated -= OnLoginMethodsUpdated;
            
            CleanUpPreviews();
        }

        private void OnLoginMethodsUpdated()
        {
            var methodPreviewModels = ContextData.GetModels();

            CleanUpPreviews();
            SetupPreviews(methodPreviewModels);
        }

        private void SetupPreviews(IEnumerable<ILoginMethodInfo> models)
        {
            _loginMethods.Values.ForEach(view => view.SetActive(false));

            models.ForEach(model =>
            {
                if (!_loginMethods.TryGetValue(model.Type, out var view)) return;

                var label = _localization.GetCredentialsLabel(model);
                var viewModel = new LoginMethodPreviewModel(model, label);
                
                view.SetActive(true);
                view.Initialize(viewModel);

                view.NextButtonClicked += OnNextButtonClicked;
            });
        }

        private void CleanUpPreviews()
        {
            _loginMethods.Values.ForEach(view =>
            {
                if (view.IsInitialized)
                {
                    view.CleanUp();
                }

                view.NextButtonClicked -= OnNextButtonClicked;
            });
        }

        private void OnNextButtonClicked(ILoginMethodInfo verificationMethod)
        {
            NextButtonClicked?.Invoke(verificationMethod);
        }

        [Serializable]
        internal sealed class LoginMethodsDictionary : SerializedDictionary<CredentialType, LoginMethodPreview>
        {
        }
    }
}