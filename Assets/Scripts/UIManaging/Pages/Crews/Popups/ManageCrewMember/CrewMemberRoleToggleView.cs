using System;
using Abstract;
using Common;
using Modules.Crew;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Popups.ManageCrewMember
{
    internal sealed class CrewMemberRoleToggleModel
    {
        public readonly long RoleId;
        public readonly string RoleName;
        public readonly ToggleGroup ToggleGroup;
        public readonly bool IsCurrentRole;

        public CrewMemberRoleToggleModel(long roleId, string roleName, ToggleGroup toggleGroup, bool isCurrentRole)
        {
            RoleId = roleId;
            RoleName = roleName;
            ToggleGroup = toggleGroup;
            IsCurrentRole = isCurrentRole;
        }
    }
    
    internal sealed class CrewMemberRoleToggleView : BaseContextDataView<CrewMemberRoleToggleModel>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _roleName;
        
        public Action<long> OnSelected;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void OnDisable()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }
        
        
        public void MarkAsNotInteractable()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.alpha = 0.5f;
            _toggle.onValueChanged.RemoveAllListeners();
        }

        public void SetInteractable(bool value)
        {
            _toggle.interactable = value;
            _toggle.SetIsOnWithoutNotify(!value);
        }
        
        protected override void OnInitialized()
        {
            _roleName.text = ContextData.RoleName;
            SetInteractable(!ContextData.IsCurrentRole);
        }

        private void OnToggleValueChanged(bool value)
        {
            SetInteractable(value);           
            OnSelected?.Invoke(ContextData.RoleId);
        }
    }
}