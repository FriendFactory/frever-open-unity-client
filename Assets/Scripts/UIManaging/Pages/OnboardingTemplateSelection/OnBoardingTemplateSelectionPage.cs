using System;
using System.Threading;
using Bridge.Models.ClientServer.Template;
using Bridge;
using Modules.Amplitude;
using Modules.InputHandling;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnboardingTemplateSelection
{
    internal sealed class OnBoardingTemplateSelectionPage : GenericPage<OnboardingTemplateSelectionArgs>
    {
        [SerializeField] private CarouselTemplateList _carouselTemplateList;
        [SerializeField] private Button _levelEditorButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private int _templatesCount;

        [Inject] private readonly IBridge _bridge;
        [Inject] private IInputManager _inputManager;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IUniverseManager _universeManager;

        private TemplateInfo[] _templateInfos;
        private CancellationTokenSource _cancellationToken;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.OnBoardingTemplateSelection;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _levelEditorButton.onClick.AddListener(OpenLevelEditor);
            _skipButton.onClick.AddListener(OnSkipClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _cancellationToken.Cancel();
            _cancellationToken = null;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {

        }
        
        protected override void OnDisplayStart(OnboardingTemplateSelectionArgs args)
        {
            base.OnDisplayStart(args);
            _inputManager.Enable(true);
            _skipButton.onClick.AddListener(OnSkipClicked);
            _skipButton.gameObject.SetActive(ShowSkipButton());
            SetupTemplates();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _inputManager.Enable(true);
            _skipButton.onClick.RemoveListener(OnSkipClicked);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void SetupTemplates()
        {
            _cancellationToken = new CancellationTokenSource();
            
            var templates = await _bridge.GetOnBoardingEventTemplates(_templatesCount, 0, _cancellationToken.Token);
            if (!templates.IsSuccess || templates.IsRequestCanceled) return;
            _templateInfos = templates.Models;
            DisplayTemplates();
        }

        private void DisplayTemplates()
        {
            if (_templateInfos == null) return;

            var rectTransform = (RectTransform)_levelEditorButton.transform;
            var cellSize = rectTransform.rect.width;
            var carouselListModel = new CarouselTemplateListModel(_templateInfos, cellSize);
            _carouselTemplateList.Initialize(carouselListModel);
            
            OnPageOpened();
        }

        private void OpenLevelEditor()
        {
            var selectedTemplateInfo = _carouselTemplateList.SelectedTemplateInfo ?? _templateInfos[0];

            _universeManager.SelectUniverse(universe =>
            {
                var args = new CreateNewLevelBasedOnTemplateScenarioArgs
                {
                    Universe = universe,
                    Template = selectedTemplateInfo
                };
                _scenarioManager.ExecuteNewLevelCreationBasedOnTemplate(args); // TODO: remove this page and this scenario call when new onboarding logic is implemented
            });
        }

        //Check if user is part of amplitude A/B test and in treatment group for enabling skip button
        private bool ShowSkipButton()
        {
            const string activateSkipButtonVariantName = "treatment";

            var variantValue = AmplitudeManager.GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.SKIP_ONBOARDING_TEMPLATE_SELECTION);
            return variantValue == activateSkipButtonVariantName;
        }
        
        private void OnSkipClicked()
        {
            OpenPageArgs.OnSkipButtonClicked?.Invoke();
        }
    }
}