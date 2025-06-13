using System.Threading;
using Common.UI;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal abstract class MusicViewBase<TModel>: UIElementWithPlaceholder<TModel> where TModel: MusicViewModel
    {
        [SerializeField] private PageHeaderView _pageHeaderView;

        [Inject] protected MusicSelectionStateController _stateSelectionController;

        public bool Initialized => IsInitialized;
        
        protected abstract string Name { get; }

        protected override InitializationResult OnInitialize(TModel model, CancellationToken token)
        {
            var pageHeaderArgs = new PageHeaderArgs(Name, new ButtonArgs(string.Empty, MoveBack));
            _pageHeaderView.Init(pageHeaderArgs);
            
            OnInitialized(model);

            return InitializationResult.Done;
        }

        protected override void OnInitializationCancelled() { }
        protected virtual void OnInitialized(TModel model) { }

        protected override void OnShowContent()
        {
            gameObject.SetActive(true);
        }
        
        protected override void OnHideContent()
        {
            gameObject.SetActive(false);
        }

        protected override void OnCleanUp() { }

        protected virtual void MoveBack() => _stateSelectionController.ChangeToPreviousState();
    }
}