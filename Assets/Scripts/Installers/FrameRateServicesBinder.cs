using Common;
using Modules.FrameRate;
using Navigation.Core;
using Zenject;

namespace Installers
{
    internal static class FrameRateServicesBinder
    {
        private static readonly PageId[] HEAVY_PAGES = { PageId.LevelEditor, PageId.PostRecordEditor, PageId.PublishPage };
        
        public static void BindFrameRateServices(this DiContainer container)
        {
            #if UNITY_IOS
            var isLowEndDevice = !DeviceInformationHelper.DeviceSupportsTrueDepth();
            if (isLowEndDevice)
            {
                container.BindInterfacesAndSelfTo<KeepConstantFrameRateControl>().AsSingle().WithArguments(30);
                return;
            }
            #endif
            
            container.BindInterfacesAndSelfTo<KeepHighFrameRateOnLightPagesControl>().AsSingle().WithArguments(HEAVY_PAGES);
        }
    }
}