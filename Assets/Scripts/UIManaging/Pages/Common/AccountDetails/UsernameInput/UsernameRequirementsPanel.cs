using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using Extensions;
using Modules.SignUp;
using UIManaging.Common.InputFields;
using UIManaging.Common.InputFields.Requirements;
using UIManaging.Pages.Common.AccountDetails.UsernameInput;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    public class UsernameRequirementsPanel: BaseContextlessPanel
    {
        [SerializeField] private RequirementTypesDictionary _requirementTypesMap;
        
        private readonly Dictionary<RequirementType, InputRequirement> _requirementsMap = new()
        {
            { RequirementType.CharacterLimit, new InputRequirement()},
            { RequirementType.SpecialCharacters, new InputRequirement()},
            { RequirementType.PersonalInfo, new InputRequirement()},
        };

        public void ChangeRequirementsState(RequirementValidationState state)
        {
            _requirementsMap.Values.ForEach(requirement => requirement.ValidationState = state);
        }

        public void UpdateRequirements(Dictionary<RequirementType, bool> failedRequirements)
        {
            foreach (var kvp in _requirementsMap)
            {
                kvp.Deconstruct(out var type, out var requirement);

                if (!failedRequirements.TryGetValue(type, out var validationResult)) return;

                requirement.ValidationState = validationResult
                    ? RequirementValidationState.Invalid
                    : RequirementValidationState.Valid;
            }
        }
        
        protected override void OnInitialized()
        {
            _requirementsMap.Values.ForEach(requirement => requirement.ValidationState = RequirementValidationState.Valid);
            _requirementTypesMap.ForEach(kvp =>
            {
                kvp.Deconstruct(out var type, out var view);

                if (!_requirementsMap.TryGetValue(type, out var model))
                {
                    view.SetActive(false);
                    return;
                }
                
                view.Initialize(model);
            });
        }

        protected override void BeforeCleanUp()
        {
            _requirementTypesMap.Values.Where(view => view.IsInitialized).ForEach(view => view.CleanUp());
        }
    }
}