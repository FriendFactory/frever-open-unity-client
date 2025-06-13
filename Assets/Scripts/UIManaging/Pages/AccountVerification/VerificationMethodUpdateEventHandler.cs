using System;
using JetBrains.Annotations;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.LoginMethods;
using Navigation.Core;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.AppSettingsPage.UI.Args;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AccountVerification
{
    [UsedImplicitly]
    internal sealed class VerificationMethodUpdateEventHandler: IInitializable, IDisposable
    {
        private readonly PageManager _pageManager;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly AccountVerificationLocalization _localization;
        private readonly LoginMethodsProvider _loginMethodManager;

        public VerificationMethodUpdateEventHandler(PageManager pageManager, SnackBarHelper snackBarHelper, AccountVerificationLocalization localization, LoginMethodsProvider loginMethodManager)
        {
            _pageManager = pageManager;
            _snackBarHelper = snackBarHelper;
            _localization = localization;
            _loginMethodManager = loginMethodManager;
        }

        public void Initialize()
        {
            StaticBus<VerificationMethodUpdatedEvent>.Subscribe(OnMethodUpdated);
            
        }

        public void Dispose()
        {
            StaticBus<VerificationMethodUpdatedEvent>.Unsubscribe(OnMethodUpdated);
        }

        private async void OnMethodUpdated(VerificationMethodUpdatedEvent @event)
        {
            try
            {
                var result = @event.Result;

                if (result.IsSuccess)
                {
                    await _loginMethodManager.UpdateLoginMethodsAsync();
                }

                if (_pageManager.CurrentPage.Id != PageId.ManageAccountPage)
                {
                    _pageManager.TryMoveBackTo(PageId.ManageAccountPage, new ManageAccountPageArgs());
                }

                var message = !string.IsNullOrEmpty(result.Message)
                    ? result.Message
                    : _localization.GetMethodUpdatedMessage(result.Type, result.OperationType, result.IsSuccess);

                if (result.IsSuccess)
                {
                    _snackBarHelper.ShowSuccessDarkSnackBar(message);
                }
                else
                {
                    _snackBarHelper.ShowFailSnackBar(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}