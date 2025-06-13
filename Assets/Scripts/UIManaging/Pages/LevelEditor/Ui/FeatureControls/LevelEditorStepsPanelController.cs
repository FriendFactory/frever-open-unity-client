using System;
using System.Linq;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    public class LevelEditorStepsPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _panels;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private EditingStep[] _steps;

        private IOrderedEnumerable<EditingStep> Steps => _steps.OrderBy(x => x.OrderIndex);
        
        private int _currentStep = -1;

        private void Awake()
        {
            _nextButton.onClick.AddListener(NextStep);
            _backButton.onClick.AddListener(PrevStep);
            NextStep();
        }

        private void NextStep()
        {
            _currentStep++;
            EnableCurrentStepPanel();
        }

        private void PrevStep()
        {
            _currentStep--;
            EnableCurrentStepPanel();
        }

        private void EnableCurrentStepPanel()
        {
            for (int i = 0; i < _panels.Length; i++)
            {
                _panels[i].gameObject.SetActive(i == _currentStep);
            }
            
            Steps.ElementAt(_currentStep).Run();

            _nextButton.SetActive(_currentStep != _steps.Length - 1);
            _backButton.SetActive(_currentStep != 0);
        }
    }
}