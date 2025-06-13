using Bridge.Models;
using UnityEngine;
using Space = Bridge.Models.ClientServer.Assets.Space;

namespace Modules.LevelManaging.Assets.AssetHelpers
{
    public sealed class VfxPositionAdjuster : MonoBehaviour
    {
        public bool TrackRotation { get; set; } = true;
        private Transform _targetTransform;
        public Vector3? AdjustPosition { get; set; }
        public Space? PositionSpace { get; set; }
        public Vector3? AdjustRotation { get; set; }

        private ICharacterAsset _characterAsset;
        private string _anchorPoint;
       
        void LateUpdate()
        {
            if (_targetTransform == null) return;
            UpdatePosition();
        }

        public void SetTargetCharacter(ICharacterAsset characterAsset, string anchorPoint)
        {
            if (_characterAsset != null)
            {
                _characterAsset.Updated -= OnCharacterUpdated;
            }
            
            _characterAsset = characterAsset;
            _anchorPoint = anchorPoint;
            _characterAsset.Updated += OnCharacterUpdated;
            
            _targetTransform = GetCharacterBoneTargetTransform(anchorPoint, characterAsset);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector3 nextPos;

            if (!AdjustPosition.HasValue)
            {
                nextPos = _targetTransform.position;
            }
            else if (PositionSpace == Space.World)
            {
                nextPos = _targetTransform.position + AdjustPosition.Value;
            }
            else
            {
                nextPos = _targetTransform.TransformPoint(AdjustPosition.Value);
            }
            
            transform.position = nextPos;

            if (!TrackRotation) return;
            
            transform.rotation = _targetTransform.rotation;
            if (AdjustRotation.HasValue)
            {
                transform.rotation *= Quaternion.Euler(AdjustRotation.Value);
            }
        }

        private void OnDestroy()
        {
            if (_characterAsset == null) return;
            _characterAsset.Updated -= OnCharacterUpdated;
        }

        private void OnCharacterUpdated()
        {
            _targetTransform = GetCharacterBoneTargetTransform(_anchorPoint, _characterAsset);
            UpdatePosition();
        }
        
        private static Transform GetCharacterBoneTargetTransform(string anchorPoint, ICharacterAsset character)
        {
            var target = anchorPoint switch
            {
                ServerConstants.VfxAnchorNames.RIGHT_HAND => character.RightHandBoneGameObject.transform,
                ServerConstants.VfxAnchorNames.LEFT_HAND => character.LeftHandBoneGameObject.transform,
                ServerConstants.VfxAnchorNames.SPINE => character.SpineBoneGameObject.transform,
                ServerConstants.VfxAnchorNames.MOUTH => character.MouthBoneGameObject.transform,
                ServerConstants.VfxAnchorNames.ROOT => character.View.GameObject.transform,
                ServerConstants.VfxAnchorNames.HEAD => character.HeadBoneGameObject.transform,
                _ => character.HeadBoneGameObject.transform
            };

            return target;
        }
    }
}
