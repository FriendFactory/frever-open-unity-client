using System;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Extensions;
using TMPro;
using UIManaging.Common.SelectionPanel;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class OnboardingContactsContinueButton: BaseContextDataButton<OnboardingContactsContinueButtonArgs>
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _loadingIndicator;

        [Inject] private IBridge _bridge;
        [Inject] private OnBoardingLocalization _localization;
        
        protected override void OnInitialized()
        {
            UpdateButtonText();
            ContextData.SelectionPanelModel.ItemSelectionChanged += OnSelectionChanged;
        }

        protected override void BeforeCleanup()
        {
            if (ContextData?.SelectionPanelModel == null) return;
            
            ContextData.SelectionPanelModel.ItemSelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(ISelectionItemModel _)
        {
            UpdateButtonText();
        }
        
        private void UpdateButtonText()
        {
            var selectedItemsCount = ContextData.SelectionPanelModel.SelectedItems.Count;
            _label.text = selectedItemsCount > 0 
                ? string.Format(_localization.FollowButtonFormat, selectedItemsCount.ToString()) 
                : _localization.ContinueButton.ToString();
        }

        protected override async void OnUIInteracted()
        {
            base.OnUIInteracted();

            SetLoading(true);

            try
            {
                var startFollowTasks = ContextData.SelectionPanelModel.SelectedItems
                                                  .Select(selected => _bridge.StartFollow(selected.Id));

                await Task.WhenAll(startFollowTasks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                SetLoading(false);
                ContextData.OnContinueButtonClick?.Invoke();
            }
        }

        private void SetLoading(bool state)
        {
            _label.SetActive(!state);
            _loadingIndicator.SetActive(state);
        }
    }
}