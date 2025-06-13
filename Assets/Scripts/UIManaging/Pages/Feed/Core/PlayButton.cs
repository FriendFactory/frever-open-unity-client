using Bridge.Models.ClientServer.Template;
using Modules.FeaturesOpening;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using TMPro;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    public sealed class PlayButton : TemplateUsedForVideoButton
    {
        [Inject] private IAppFeaturesManager _featuresManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IUniverseManager _universeManager;
        
        //TODO: consider to use localization engine if the feature is gonna be published
        private const string PLAY_LABEL = "Play";

        [SerializeField] private TMP_Text _label;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Initialize(long templateId, string templateTitle, UnityAction onClick)
        {
            base.Initialize(templateId, templateTitle, onClick);

            _label.text = PLAY_LABEL;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_label.rectTransform);
        }

        protected override void OnClick()
        {
            if (!_featuresManager.IsCreationNewLevelAllowed)
            {
                _popupManagerHelper.ShowLockedLevelCreationFeaturePopup(_featuresManager.ChallengesRemainedForEnablingNewLevelCreation);
                return;
            }

            _universeManager.SelectUniverse(universe =>
            {
                Button.interactable = false;

                var args = new CreateNewLevelBasedOnTemplateScenarioArgs
                {
                    Template = new TemplateInfo { Id = TemplateID },
                    OnCancelCallback = () => Button.interactable = true,
                    Universe = universe
                };
                _scenarioManager.ExecuteNewLevelCreationBasedOnTemplate(args);
            });
        }
    }
}