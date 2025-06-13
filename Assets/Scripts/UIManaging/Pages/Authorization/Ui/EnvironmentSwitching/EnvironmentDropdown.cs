using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Authorization.Ui.LoginLogic
{
    public sealed class EnvironmentDropdown : MonoBehaviour
    {
        private const FFEnvironment DEFAULT_ENVIRONMENT = FFEnvironment.Production;

        [SerializeField] private Dropdown _dropdown;

        [Inject] private IBridge _bridge;
        private List<FFEnvironment> _availableEnvironments;

        private bool _initialized;

        private void Awake()
        {
            Hide();
        }

        public void Hide()
        {
            _dropdown.gameObject.SetActive(false);
        }

        public async void Show()
        {
            _dropdown.gameObject.SetActive(true);

            if (_initialized) return;
            await Setup();
            _initialized = true;
        }

        private async Task Setup()
        {
            await GetCompatibleEnvironments();
            var options = _availableEnvironments.Select(env => env.ToString()).ToList();
            _dropdown.ClearOptions();

            if (options.Count == 0)
            {
                _dropdown.AddOptions(new List<string> { "No compatible env" });
                return;
            }

            _dropdown.AddOptions(options);
            var defaultValue = _availableEnvironments.Contains(DEFAULT_ENVIRONMENT)
                ? _availableEnvironments.IndexOf(DEFAULT_ENVIRONMENT)
                : 0;
            _dropdown.value = defaultValue;
            ChangeEnvironment(_dropdown.value);
            _dropdown.onValueChanged.AddListener(ChangeEnvironment);
        }

        private async Task GetCompatibleEnvironments()
        {
            var results = await _bridge.GetEnvironmentsCompatibilityData();

            if (results.IsSuccess)
            {
                _availableEnvironments = results.Models
                                                .Where(envResult => envResult.IsCompatibleWithBridge)
                                                .Select(envResult => envResult.Environment).ToList();
            }
        }

        private void ChangeEnvironment(int index)
        {
            if (_bridge.IsLoggedIn) return;

            var environment = _availableEnvironments[index];
            _bridge.ChangeEnvironment(environment);
        }
    }
}
