using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    internal sealed class EditUsernamePresenter: GenericPresenter<EditUsernameModel, EditUsernameView>
    {
        private readonly PageManager _pageManager;
        
        public EditUsernamePageArgs EditUsernamePageArgs { get; set; }

        public EditUsernamePresenter(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override void OnInitialized()
        {
            View.MoveBackRequested += MoveBack;
            View.UpdateRequested += OnUpdateRequested;
        }

        protected override void BeforeCleanUp()
        {
            View.MoveBackRequested -= MoveBack;
            View.UpdateRequested -= OnUpdateRequested;
        }

        private void OnUpdateRequested(string username)
        {
            EditUsernamePageArgs.UpdateRequested?.Invoke(username);
        }

        private void MoveBack() => _pageManager.MoveBack();
    }
}