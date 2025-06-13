using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.Common;
using Models;
using static Extensions.DbModelType;

namespace Extensions
{
    public static partial class EventExtensions 
    {
        public static List<IPurchasable> GetStoreAssets(this Event ev, PurchasedAssetsData purchasedAssets)
        {
            ThrowExceptionIfEventIsNull(ev);

            var list = new List<IPurchasable>();

            var purchasedVfx = purchasedAssets.GetPurchasedAssetsByType(Vfx);
            var purchasedBodyAnimations = purchasedAssets.GetPurchasedAssetsByType(BodyAnimation);
            var purchasedCameraFilters = purchasedAssets.GetPurchasedAssetsByType(CameraFilter);
            var purchasedSetLocations = purchasedAssets.GetPurchasedAssetsByType(SetLocation);

            AddVfxToList(ev, list, purchasedVfx);
            AddBodyAnimationsToList(ev, list, purchasedBodyAnimations);
            AddCameraFilterToList(ev, list, purchasedCameraFilters);
            AddSetLocationToList(ev, list, purchasedSetLocations);

            return list;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void AddVfxToList(Event ev, ICollection<IPurchasable> list, long[] purchasedIds)
        {
            var vfx = ev.GetVfx();
            if (vfx?.AssetOffer != null && purchasedIds != null && !purchasedIds.Contains(vfx.Id))
            {
                list.Add(vfx);
            }
        }

        private static void AddBodyAnimationsToList(Event ev, ICollection<IPurchasable> list, long[] purchasedIds)
        {
            foreach (var controller in ev.GetCharacterBodyAnimationControllers())
            {
                var bodyAnimation = controller.BodyAnimation;
                if (bodyAnimation?.AssetOffer != null && purchasedIds != null && !purchasedIds.Contains(bodyAnimation.Id))
                {
                    list.Add(bodyAnimation);
                }
            }
        }

        private static void AddCameraFilterToList(Event ev, ICollection<IPurchasable> list, long[] purchasedIds)
        {
            var cameraFilter = ev.GetCameraFilter();
            if (cameraFilter?.AssetOffer != null && purchasedIds != null && !purchasedIds.Contains(cameraFilter.Id))
            {
                list.Add(cameraFilter);
            }
        }

        private static void AddSetLocationToList(Event ev, ICollection<IPurchasable> list, long[] purchasedIds)
        {
            var setLocation = ev.GetSetLocation();
            if (setLocation?.AssetOffer != null && purchasedIds != null && !purchasedIds.Contains(setLocation.Id))
            {
                list.Add(setLocation);
            }
        }
    }
}