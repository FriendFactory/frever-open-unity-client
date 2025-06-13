using System;
using Common.Abstract;
using Extensions;
using Modules.AccountVerification.LoginMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    internal sealed class LoginMethodPreview : BaseContextPanel<LoginMethodPreviewModel>
    {
        [SerializeField] private GameObject _dotIndicator;
        [SerializeField] private TMP_Text _credentials;
        [SerializeField] private Button _nextButton;
        [SerializeField] private GameObject _arrow;

        public event Action<ILoginMethodInfo> NextButtonClicked;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            _credentials.text = ContextData.Label;
            var loginMethodInfo = ContextData.LoginMethodInfo;

            _dotIndicator.SetActive(!loginMethodInfo.IsLinked);
            _nextButton.SetActive(loginMethodInfo.IsChangeable);
            _arrow.SetActive(loginMethodInfo.IsChangeable || !loginMethodInfo.IsLinked);

            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        protected override void BeforeCleanUp()
        {
            _nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        private void OnNextButtonClicked()
        {
            NextButtonClicked?.Invoke(ContextData.LoginMethodInfo);
        }
    }
}