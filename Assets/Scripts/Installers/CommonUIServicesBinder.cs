using Common;
using Navigation.Core;
using UIManaging.Animated;
using UIManaging.Common.InputFields;
using UIManaging.Common.RankBadge;
using UIManaging.Pages.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class CommonUIServicesBinder
    {
        public static void BindCommonUIServices(this DiContainer container)
        {
            var pageManager = Object.FindObjectOfType<PageManager>();
            var popupManager = Object.FindObjectOfType<PopupManager>();
            var snackBarManager = Object.FindObjectOfType<SnackBarManager>();
            var rankBadgeManager = Object.FindObjectOfType<RankBadgeManager>();
            
            container.BindInterfacesAndSelfTo<PageManager>().FromInstance(pageManager);
            container.Bind<PageManagerHelper>().AsSingle();
            container.Bind<PopupManager>().FromInstance(popupManager).AsSingle();
            container.Bind<PopupManagerHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<PopupParentManager>().AsSingle();
            container.Bind<SnackBarManager>().FromInstance(snackBarManager).AsSingle();
            container.Bind<SnackBarHelper>().AsSingle();
            container.Bind<InputFieldAdapterFactory>().AsSingle();
            container.Bind<StartAwaiterHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<NativeKeyboardHeightProvider>().AsSingle();
            container.Bind<RankBadgeManager>().FromInstance(rankBadgeManager).AsSingle();
        }
    }
}
