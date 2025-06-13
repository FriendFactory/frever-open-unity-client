using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    public static class CameraFilterControllerExtension
    {
        public static void SetCameraFilterVariant(this CameraFilterController controller, CameraFilterInfo filter, long variantId)
        {
            if (filter.CameraFilterVariants.All(x => x.Id != variantId))
                throw new InvalidOperationException($"Filter {filter.Id} doesn't contain filter variant {variantId}");
                
            controller.CameraFilterVariantId = variantId;
            controller.CameraFilter = filter;
        }
    }
}