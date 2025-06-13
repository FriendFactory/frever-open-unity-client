using System;
using Bridge.Models.ClientServer.Crews;
using EnhancedUI.EnhancedScroller;
using UIManaging.EnhancedScrollerComponents;
using UnityEngine;

namespace UIManaging.Pages.DiscoveryPage
{
    public class CrewsListView : BaseEnhancedScrollerView<CrewItemView, CrewShortInfo>
    {
        public event Action<CrewShortInfo> OnCellClick;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            OnCellViewInstantiatedEvent += SubscribeToClick;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            OnCellViewInstantiatedEvent -= SubscribeToClick;
        }

        private void SubscribeToClick(CrewItemView cell)
        {
            cell.OnClick += OnCellClicked;
        }
        private void OnCellClicked(CrewShortInfo cellModel)
        {
            OnCellClick?.Invoke(cellModel);
        }

    }
}