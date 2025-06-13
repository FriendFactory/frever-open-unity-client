using System;
using Bridge;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    public class InvitationBridgeHelper: MonoBehaviour
    {
        [SerializeField] private string _invitationCode = "xyz90zyx";
        [SerializeField] private string _invitationGuid = "086e4c81-055c-47cb-b685-ac60141068d6";

        [Inject] private IBridge _bridge;

        [Button]
        private async void SaveInvitationCode()
        {
            var result = await _bridge.SaveInvitationCode(_invitationCode);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to save invitation code # {result.ErrorMessage}");
                return;
            }

            Debug.Log($"[{GetType().Name}] Invitation code has saved # {_invitationCode}");
        }

        [Button]
        private async void GetInvitationCode()
        {
            var result = await _bridge.GetInvitationCode();
            if (result.IsError)
            {
                Debug.Log($"[{GetType().Name}] Failed to get invitation code # {result.ErrorMessage}");
                return;
            }

            Debug.Log($"[{GetType().Name}] Invitation code is # {result.Model.Code} {result.Model.InvitationGuid}");
            
            if (!Guid.TryParse(_invitationGuid, out var guid))
            {
                Debug.LogError($"[{GetType().Name}] Failed to parse guid");
                return;
            }
            
            Debug.Log($"[{GetType().Name}] Guids are equal: {result.Model.InvitationGuid == guid}");
        }

        [Button]
        private async void UseInvitationCode()
        {
            if (!Guid.TryParse(_invitationGuid, out var guid))
            {
                Debug.LogError($"[{GetType().Name}] Failed to parse guid");
                return;
            }
            
            var result = await _bridge.UseInvitationCode(guid);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to use invitation code # {result.ErrorMessage}");
                return;
            }

            Debug.Log($"[{GetType().Name}] Invitation code has used # {result.Model.InviterNickName}");
        }

    }
}