using System.Linq;
using Extensions;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing
{
    internal sealed class CameraFocusManager
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly EventAssetsProvider _eventAssetsProvider;

        public CameraFocusManager(ICameraSystem cameraSystem, EventAssetsProvider eventAssetsProvider)
        {
            _cameraSystem = cameraSystem;
            _eventAssetsProvider = eventAssetsProvider;
        }

        public void UpdateCameraFocus(Event targetEvent, bool recenter)
        {
            GameObject lookAtTarget;
            GameObject followTarget;
            
            if (targetEvent.TargetCharacterSequenceNumber < 0)
            {
                lookAtTarget = _cameraSystem.GetLookAtTargetGroupGameObject();
                followTarget = _cameraSystem.GetFollowTargetGroupGameObject();
                RefreshCameraGroupPosition(targetEvent);
            }
            else
            {
                var focusedSeqNumb = targetEvent.TargetCharacterSequenceNumber;
                var focusedCharacterId = targetEvent.GetCharacterController(focusedSeqNumb).CharacterId;
                var character =
                    _eventAssetsProvider.GetLoadedAssets(targetEvent, DbModelType.Character)
                        .First(x => x.Id == focusedCharacterId) as ICharacterAsset;
                lookAtTarget = character?.LookAtBoneGameObject;
                followTarget = character?.GameObject;
            }
            
            _cameraSystem.SetTargets(lookAtTarget, followTarget, recenter);
        }
        
        public void RefreshCameraGroupPosition(Event ev)
        {
            var characterAssets = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Character).Cast<ICharacterAsset>();

            var characterLookAtTransforms = characterAssets.Select(x => x.LookAtBoneGameObject.transform);
            _cameraSystem.SetLookAtGroupMembers(characterLookAtTransforms);
            
            var characterFollowTransforms = characterAssets.Select(x => x.GameObject.transform);
            _cameraSystem.SetFollowGroupMembers(characterFollowTransforms);
        }
    }
}