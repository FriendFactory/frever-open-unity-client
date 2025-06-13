using System;
using Modules.Crew;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Crews.Sidebar;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    [RequireComponent(typeof(CanvasGroup))]
    internal class RoleControlledSelectable : MonoBehaviour
    {
        private const float LOCKED_ALPHA = 0.25f;

        [SerializeField] protected Selectable _selectable;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Role _unlocksAt;
        
        [Inject] protected CrewService _crewService;
        [Inject] private LocalUserDataHolder _localUser;
        

        private void OnEnable()
        {
            _crewService.CrewModelUpdated += _ => Refresh();
            
            Refresh();
        }

        private void OnDisable()
        {
            _crewService.CrewModelUpdated -= _ => Refresh();
        }

        protected virtual void Refresh()
        {
            if (_crewService.LocalUserMemberData.RoleId <= (long)_unlocksAt)
            {
                _canvasGroup.alpha = 1.0f;
                _selectable.interactable = true;
                return;
            }

            _canvasGroup.alpha = LOCKED_ALPHA;
            _selectable.interactable = false;
        }
    }
}