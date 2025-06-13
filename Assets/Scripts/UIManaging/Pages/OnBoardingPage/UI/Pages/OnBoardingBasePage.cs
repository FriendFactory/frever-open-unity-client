using System;
using Navigation.Core;
using TMPro;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using UnityEngine.UI;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    public abstract class OnBoardingBasePage<T> : GenericPage<T> where T : OnBoardingBasePageArgs
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] protected Button ContinueButton;
        
        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(T args)
        {
            base.OnDisplayStart(args);
            args.DisplayStart?.Invoke();
            _titleText.text = OpenPageArgs.TitleText;
            _descriptionText.text = OpenPageArgs.DescriptionText;
            
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _backButton.gameObject.SetActive(args.ShowBackButton);
            
            if(args.ShowBackButton)
            {
                _backButton.onClick.AddListener(OnBackButtonClicked);
            }
            
            RefreshContinueButtonInteractivity();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            OpenPageArgs.RequestMoveBack?.Invoke();
        }

        protected void RefreshContinueButtonInteractivity()
        {
            if (OpenPageArgs?.InputCheckerFunction != null)
            {
                ContinueButton.interactable = OpenPageArgs.InputCheckerFunction.Invoke();
            }
        }
        
        protected virtual void OnContinueButtonClicked()
        {
            OpenPageArgs.OnContinueButtonClick?.Invoke();
            ContinueButton.interactable = false;
        }

        protected virtual void OnEnable()
        {
            RefreshContinueButtonInteractivity();
            ContinueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        protected virtual void OnDisable()
        {
            ContinueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
    }
}