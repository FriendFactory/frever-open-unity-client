using System.Collections.Generic;
using Abstract;
using Common;
using Modules.Crew;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Popups.ManageCrewMember
{
    internal sealed class CrewMemberRolesSectionModel
    {
        public readonly long MemberId;
        public long RoleId;

        public CrewMemberRolesSectionModel(long memberId, long roleId)
        {
            MemberId = memberId;
            RoleId = roleId;
        }
    }
    internal sealed class CrewMemberRoleSection : BaseContextDataView<CrewMemberRolesSectionModel>
    {
        private static readonly long[] ROLE_TOGGLES_MAPPING = {
            Constants.Crew.LEADER_ROLE_ID,
            Constants.Crew.COORDINATOR_ROLE_ID,
            Constants.Crew.ELDER_ROLE_ID,
            Constants.Crew.MEMBER_ROLE_ID,
            Constants.Crew.RECRUIT_ROLE_ID
        };

        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Transform _togglesParent;
        [SerializeField] private CrewMemberRoleToggleView _roleToggle;

        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _localization;

        private readonly Dictionary<long, CrewMemberRoleToggleView> _roleToggles = new Dictionary<long, CrewMemberRoleToggleView>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var toggle in _roleToggles.Values)
            {
                toggle.OnSelected -= OnRoleSelected;
            }
        }

        protected override void OnInitialized()
        {
            InstantiateToggles();
            InitializeToggles();
        }

        private void InstantiateToggles()
        {
            if (_roleToggles.Count != 0) return;

            foreach (var roleId in ROLE_TOGGLES_MAPPING)
            {
                var toggle = Instantiate(_roleToggle, _togglesParent);
                _roleToggles.Add(roleId, toggle);
                toggle.OnSelected += OnRoleSelected;
            }
        }

        private void InitializeToggles()
        {
            foreach (var roleId in ROLE_TOGGLES_MAPPING)
            {
                var toggle = _roleToggles[roleId];
                var isCurrentRole = roleId == ContextData.RoleId;
                var model = new CrewMemberRoleToggleModel(roleId,_localization.GetRankLocalized(roleId), _toggleGroup, isCurrentRole);
                
                if (roleId == Constants.Crew.LEADER_ROLE_ID) toggle.MarkAsNotInteractable();
                
                toggle.Initialize(model);
            }
        }

        private void OnRoleSelected(long roleId)
        {
            ContextData.RoleId = roleId;
            InitializeToggles();
            _crewService.ChangeMemberRole(ContextData.MemberId, roleId);
        }
    }
}