using System.Linq;
using Bridge.Models;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using UnityEngine;

namespace Modules.LevelManaging.Assets.AssetHelpers
{
    public sealed class VfxBinder
    {
        private readonly IMetadataProvider _metadataProvider;

        public VfxBinder(IMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
        }
        
        public void Setup(IVfxAsset vfxAsset, ICharacterAsset characterAsset)
        {
            var anchorPoint = vfxAsset.RepresentedModel.AnchorPoint;
            if (string.IsNullOrEmpty(anchorPoint)) return;
          
            var assetPositionAdjuster = vfxAsset.GameObject.AddOrGetComponent<VfxPositionAdjuster>();
            assetPositionAdjuster.TrackRotation = vfxAsset.RepresentedModel.FollowRotation;
            
            assetPositionAdjuster.SetTargetCharacter(characterAsset, anchorPoint);
            var adjustment = vfxAsset.RepresentedModel.Adjustments?.FirstOrDefault(x => x.GenderIds.Contains(characterAsset.GenderId));
            assetPositionAdjuster.PositionSpace = adjustment?.Space;
            assetPositionAdjuster.AdjustPosition = adjustment?.AdjustPosition.ToUnityVector3();
            assetPositionAdjuster.AdjustRotation = adjustment?.AdjustRotation.ToUnityVector3();

            if (adjustment is { Scale: not null })
            {
                vfxAsset.GameObject.transform.localScale = Vector3.one * adjustment.Scale.Value;
            }
            
            ResetHeadAnchoredVfxChildLocalPositionIfNeeded(vfxAsset, characterAsset);
        }

        
        /// <summary>
        /// hotfix for head anchored VFXs for Sims race
        /// </summary>
        private void ResetHeadAnchoredVfxChildLocalPositionIfNeeded(IVfxAsset vfxAsset, ICharacterAsset characterAsset)
        {
            var raceId = _metadataProvider.MetadataStartPack.GetRaceByGenderId(characterAsset.GenderId).Id; 
            if (raceId != Constants.RaceIds.SIMS) return;
            
            if (vfxAsset.RepresentedModel.AnchorPoint == ServerConstants.VfxAnchorNames.HEAD
             && (vfxAsset.GameObject.transform.childCount == 1 || GetActiveChildCount(vfxAsset.GameObject.transform) == 1))
            {
                vfxAsset.GameObject.transform.GetChild(0).localPosition = Vector3.zero;
            }
        }

        //sometimes artists added extract objects, disable but forgot to delete
        private static int GetActiveChildCount(Transform t)
        {
            var activeCount = 0;
            for (var i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    activeCount++;
                }
            }

            return activeCount;
        }
    }
}
