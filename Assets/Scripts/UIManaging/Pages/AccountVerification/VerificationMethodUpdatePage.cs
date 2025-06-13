using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AccountVerification
{
    internal sealed class VerificationMethodUpdatePage: GenericPage<VerificationMethodUpdatePageArgs>
    {
        [SerializeField] private VerificationMethodUpdateView _verificationMethodUpdateView;

        private Dictionary<VerificationMethodOperationType, VerificationMethodPresenterBase> _presentersMap;
        private AccountVerificationLocalization _accountVerificationLocalization;
        private LocalUserDataHolder _localUserDataHolder;

        public override PageId Id => PageId.VerificationMethodUpdate;
        
        private VerificationMethodPresenterBase Presenter { get; set; }

        [Inject, UsedImplicitly]
        private void Construct(AddVerificationMethodPresenter addMethodPresenter, ChangeVerificationMethodPresenter changeMethodPresenter, AccountVerificationLocalization localization,
            LocalUserDataHolder localUserDataHolder)
        {
            _accountVerificationLocalization = localization;
            _localUserDataHolder = localUserDataHolder;
            
            _presentersMap = new Dictionary<VerificationMethodOperationType, VerificationMethodPresenterBase>()
            {
                { VerificationMethodOperationType.Add, addMethodPresenter },
                { VerificationMethodOperationType.Change, changeMethodPresenter },
            };
        }
        
        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(VerificationMethodUpdatePageArgs args)
        {
            base.OnDisplayStart(args);
            
            var method = args.VerificationMethod;
            var textData = _accountVerificationLocalization.GetPageTextData(method.Type, args.OperationType, _localUserDataHolder.NickName);
            var viewModel = new VerificationMethodUpdateViewModel(method, textData);

            if (!_presentersMap.TryGetValue(args.OperationType, out var presenter))
            {
                Debug.LogError($"[{GetType().Name}] Presenter for {args.OperationType} operation type is not found.");
                return;
            }

            Presenter = presenter;
            
            Presenter.Initialize(method, _verificationMethodUpdateView);
            
            _verificationMethodUpdateView.Initialize(viewModel);
            _verificationMethodUpdateView.Show();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            Presenter.CleanUp();
            
            _verificationMethodUpdateView.CleanUp();
            _verificationMethodUpdateView.Hide();
        }
    }
}