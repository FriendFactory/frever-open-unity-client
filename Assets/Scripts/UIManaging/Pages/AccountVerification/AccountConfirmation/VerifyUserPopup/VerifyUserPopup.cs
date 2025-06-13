using JetBrains.Annotations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class VerifyUserPopup: BasePopup<VerifyUserPopupConfiguration>
    {
        [SerializeField] private VerifyUserView _verifyUserView;
        
        private VerifyUserPresenter _verifyUserPresenter;
        
        [Inject, UsedImplicitly]
        private void Construct(VerifyUserPresenter verifyUserPresenter)
        {
            _verifyUserPresenter = verifyUserPresenter;
        }
        
        protected override void OnConfigure(VerifyUserPopupConfiguration configuration)
        {
            var verificationMethod = configuration.VerificationMethod;
            var userVerificationModel = new VerificationMethodUpdateFlowModel(verificationMethod, configuration.OperationType);
            
            _verifyUserPresenter.Initialize(userVerificationModel, _verifyUserView);
            
            _verifyUserView.Initialize(userVerificationModel);
            _verifyUserView.Show();
        }

        protected override void OnHidden()
        {
            _verifyUserPresenter.CleanUp();

            _verifyUserView.Hide();
            _verifyUserView.CleanUp();
        }
    }
}