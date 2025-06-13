using System.Linq;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;

#if UNITY_IOS
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace Extensions
{
    public static class FileEntityExtensions
    {
        public static FileInfo GetCompatibleBundle(this IMainFileContainable target)
        {
            #if UNITY_IOS
            return GetCompatibleBundleIOS(target);
            #elif UNITY_ANDROID
            return target.Files.FirstOrDefault(x => x.Platform == Platform.Android);
            #endif
        }

    #if UNITY_IOS
        private static FileInfo GetCompatibleBundleIOS(this IFilesAttached target)
        {
            var currentUrpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
            var isShadowsCasting = currentUrpAsset.supportsMainLightShadows;
            var bundleBuiltWithProperUrp = target.Files.FirstOrDefault(x => x.Platform == Platform.iOS
                                                 && isShadowsCasting && x.Tags.IsNullOrEmpty() 
                                                 || !isShadowsCasting && !x.Tags.IsNullOrEmpty() 
                                                                      && x.Tags.Contains(FileInfoTags.NO_SHADOWS));
            return bundleBuiltWithProperUrp != null 
                ? bundleBuiltWithProperUrp :
                target.Files.FirstOrDefault(x => x.Platform == Platform.iOS); //fallback since not all SetLocationBundle will have iOS bundles built with different URP
        }
    #endif
    }
}