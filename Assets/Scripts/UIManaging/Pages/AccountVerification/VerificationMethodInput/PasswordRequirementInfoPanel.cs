using System;
using Common.Abstract;
using Common.Collections;
using Extensions;
using TMPro;
using UIManaging.Common.InputFields.Requirements;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class PasswordRequirementInfoPanel: BaseContextPanel<PasswordRequirement>
    {
        [SerializeField] private TMP_Text _text;
        [Space]
        [SerializeField] private Color _validColor = new(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color _invalidColor = new (0.96f, 0.17f, 0.43f);
        [Space]
        [SerializeField] private RequirementValidationStatesDictionary _validationStatesMap;

        protected override void OnInitialized()
        {
            UpdateInfoState(ContextData.ValidationState);
            
            ContextData.ValidationStateChanged += OnValidationStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            ContextData.ValidationStateChanged -= OnValidationStateChanged;
        }

        private void OnValidationStateChanged(RequirementValidationState validationState) => UpdateInfoState(validationState);

        private void UpdateInfoState(RequirementValidationState validationState)
        {
            _validationStatesMap.Values.ForEach(graphic => graphic.SetActive(false));
            
            if (!_validationStatesMap.TryGetValue(validationState, out var activeGraphic)) return;
            
            activeGraphic.SetActive(true);
            
            var color = validationState == RequirementValidationState.Invalid ? _invalidColor : _validColor;
            _text.color = color;
        }
    }

    [Serializable]
    internal sealed class RequirementValidationStatesDictionary : SerializedDictionary<RequirementValidationState, Graphic>
    {
    }
}