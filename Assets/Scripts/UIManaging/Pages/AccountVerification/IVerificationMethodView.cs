using System;

namespace UIManaging.Pages.AccountVerification
{
    public interface IVerificationMethodView
    {
        event Action NextRequested;
        event Action BackRequested;

        void Show();
        void Hide();
        void ToggleLoading(bool isOn);
        void ShowValidationError(string error);
    }
}