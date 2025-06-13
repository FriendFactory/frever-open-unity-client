using System;
using JetBrains.Annotations;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.EditUsername
{
    internal sealed class EditUsernamePage : AnimatedGenericPage<EditUsernamePageArgs>
    {
        [SerializeField] private EditUsernameView _editUsernameView;
        [Inject] private EditUsernameLocalization _editUsernameLocalization;
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        private EditUsernamePresenter _editUsernamePresenter;

        public override PageId Id => PageId.EditUsername;

        [Inject, UsedImplicitly]
        private void Construct(PageManager pageManager)
        {
            _editUsernamePresenter = new EditUsernamePresenter(pageManager);
        }
    
        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(EditUsernamePageArgs args)
        {
            base.OnDisplayStart(args);
            var model = new EditUsernameModel(args.Name, _localUserDataHolder.NickName, _localUserDataHolder.UsernameUpdateAvailableOn);
            _editUsernamePresenter.Initialize(model, _editUsernameView);
            _editUsernamePresenter.EditUsernamePageArgs = args;
            _editUsernameView.Initialize(model);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
        
            _editUsernamePresenter.CleanUp();
            _editUsernameView.CleanUp();
        }
    }
}
