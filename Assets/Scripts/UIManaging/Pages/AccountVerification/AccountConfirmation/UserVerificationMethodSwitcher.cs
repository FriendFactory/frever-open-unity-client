using System;
using Bridge.AccountVerification.Models;
using Common.Abstract;
using Common.Collections;
using Extensions;
using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class UserVerificationMethodSwitcher: BaseContextView<CredentialType>
    {
        [SerializeField] private VerificationMethodButtonsDictionary _verificationMethodButtonsMap;

        [Inject] private LoginMethodsProvider _loginMethodsProvider;

        public event Action<CredentialType> VerificationMethodTypeChanged;

        protected override void OnInitialized()
        {
            _verificationMethodButtonsMap.ForEach(kvp =>
            {
                var (buttonType, button) = kvp;
                
                if (buttonType.TryConvertToLinkableType(out var linkableType) && !IsLinkableTypeLinked(linkableType))
                {
                    button.SetActive(false);
                    return;
                }
                
                button.onClick.AddListener(() => OnAnyButtonClicked(buttonType));
            });
            
            Switch(ContextData);
        }

        protected override void BeforeCleanUp()
        {
            _verificationMethodButtonsMap.Values.ForEach(button => button.onClick.RemoveAllListeners());
        }

        private void Switch(CredentialType type)
        {
            _verificationMethodButtonsMap.ForEach(kvp =>
            {
                var (buttonType, button) = kvp;
                
                if (buttonType.TryConvertToLinkableType(out var linkableType) && !IsLinkableTypeLinked(linkableType)) return;
                
                button.SetActive(true);
            });

            if (!_verificationMethodButtonsMap.TryGetValue(type, out var activeButton))
            {
                Debug.LogError($"[{GetType().Name}] Can't find corresponding switch button for {type}");
                return;
            }
            
            activeButton.SetActive(false);
        }

        private void OnAnyButtonClicked(CredentialType type)
        {
            if (!type.IsLinkable())
            {
                Switch(type);
            }
            
            VerificationMethodTypeChanged?.Invoke(type);
        }

        private bool IsLinkableTypeLinked(LinkableCredentialType type)
        {
            return type switch
            {
                LinkableCredentialType.Apple => _loginMethodsProvider.IsLinked(CredentialType.AppleId) && Application.platform == RuntimePlatform.IPhonePlayer,
                LinkableCredentialType.Google => _loginMethodsProvider.IsLinked(CredentialType.GoogleId) && Application.platform == RuntimePlatform.Android,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

    [Serializable]
    internal sealed class VerificationMethodButtonsDictionary : SerializedDictionary<CredentialType, Button>
    {
    }
}