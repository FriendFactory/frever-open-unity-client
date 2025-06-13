using Common.Abstract;
using Modules.UserScenarios.Common;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.EditUsername;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.UI.AccountManagement.Username
{
    internal sealed class UsernamePreview: BaseContextPanel<EditUsernameModel>
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _username;

        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private PageManager _pageManager;
        
        protected override void OnInitialized()
        {
            _username.text = ContextData.SelectedUsername;
            
            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        protected override void BeforeCleanUp()
        {
            _nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        private void OnNextButtonClicked()
        {
            _scenarioManager.ExecuteNicknameEditingScenario();
        }
    }
}