using System;
using Bridge.AccountVerification.Models;
using I2.Loc;
using Modules.AccountVerification.LoginMethods;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    [CreateAssetMenu(menuName = "L10N/LoginMethodsLocalization", fileName = "LoginMethodsLocalization")]
    internal sealed class LoginMethodsLocalization: ScriptableObject
    {
        [SerializeField] private LocalizedString _addLabel;
        [SerializeField] private LocalizedString _changeLabel;
        [SerializeField] private LocalizedString _linkedLabel;

        public string GetCredentialsLabel(ILoginMethodInfo loginMethodInfo)
        {
            switch (loginMethodInfo.Type)
            {
                case CredentialType.Email:
                    return loginMethodInfo.IsLinked ? loginMethodInfo.Status.Email : _addLabel;
                case CredentialType.PhoneNumber:
                    return loginMethodInfo.IsLinked ? loginMethodInfo.Status.PhoneNumber : _addLabel;
                case CredentialType.Password:
                    return loginMethodInfo.IsLinked ? _changeLabel: _addLabel;
                case CredentialType.AppleId:
                    return loginMethodInfo.IsLinked ? _linkedLabel : _addLabel;
                case CredentialType.GoogleId:
                    return loginMethodInfo.IsLinked ? _linkedLabel : _addLabel;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}