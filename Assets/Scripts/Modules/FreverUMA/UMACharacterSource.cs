using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Modules.FreverUMA
{
    internal sealed class UMACharacterSource : MonoBehaviour
    {
        public DynamicCharacterAvatar AvatarPrefab;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public DynamicCharacterAvatar SpawnCharacter(string race)
        {
            var avatar = Instantiate(AvatarPrefab);
            avatar.activeRace.name = race;
            avatar.transform.position = new Vector3(999, 999, 999);
            avatar.CharacterUpdated.AddAction(umaData =>
            {
                var sm = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
                sm.shadowCastingMode = ShadowCastingMode.On;
                sm.rootBone = avatar.transform.Find("Root").transform;
            });

            return avatar;
        }
    }
}