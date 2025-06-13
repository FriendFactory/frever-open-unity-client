using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.StartPack.Metadata;
using DTT.UI.ProceduralUI;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.Stages
{
    public sealed class BodyTypeButton : MonoBehaviour
    {
        [SerializeField] private string _genderIdentifier;
        [SerializeField] private Button _button;
        [SerializeField] private RoundedImage[] _roundedImages;
        
        [Space] 
        [SerializeField] private GameObject _notSelectedIndicator;
        [SerializeField] private GameObject _selectedIndicator;

        private readonly Dictionary<BodyTypeButtonPosition, BodyTypeButtonConfig> _configs = new()
        {
            {BodyTypeButtonPosition.Left, GenderConfigs.Left},
            {BodyTypeButtonPosition.Middle, GenderConfigs.Middle},
            {BodyTypeButtonPosition.Right, GenderConfigs.Right},
            {BodyTypeButtonPosition.TheOnly, GenderConfigs.TheOnly}
        };
        
        private Gender _gender;
        public string GenderIdentifier => _genderIdentifier;
        public event Action<Gender> ButtonClick;

        private void Awake()
        {
            _notSelectedIndicator.SetActive(true);
            _selectedIndicator.SetActive(false);
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        public void Init(Gender gender)
        {
            _gender = gender;
        }
        
        public void Refresh(Gender selectedGender)
        {
            if (selectedGender == null)
            {
                _notSelectedIndicator.SetActive(true);
                _selectedIndicator.SetActive(false);
            }
            var selected = selectedGender == _gender;

            _button.interactable = !selected;
            _notSelectedIndicator.SetActive(!selected);
            _selectedIndicator.SetActive(selected);
        }

        public void SetInteractable(bool value)
        {
            _button.interactable = value;
        }

        public void SetPositionPreset(BodyTypeButtonPosition positionType) 
        {
            if (_roundedImages is null || _roundedImages.Length <= 0)
            {
                return;
            }

            var targetConfig = _configs[positionType];
            _roundedImages.ForEach(i =>
            {
                targetConfig.Corners.ForEach(c =>
                {
                    i.SetCornerRounding(c.Corner, c.Roundness);
                });
            });
        }

        private void OnButtonClick()
        {
            ButtonClick?.Invoke(_gender);
        }
    }
}
