using System.Collections.Generic;
using System.Linq;
using Modules.InAppPurchasing;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class IAPServices
    {
        private static Dictionary<RuntimePlatform, string[]> BUNDLE_IDS_WITH_REGISTERED_PRODUCTS_IN_STORE =
            new Dictionary<RuntimePlatform, string[]>()
            {
                { RuntimePlatform.IPhonePlayer, new[] { "com.friendfactory.frever", "com.friendfactory.frever04" } },
                { RuntimePlatform.Android, new[] { "com.FriendFactory.Frever" } }
            };
        
        public static void BindIAPServices(this DiContainer container)
        {
            container.Bind<IIAPManager>().To<IAPManager>().AsSingle();
            BindStoreProductProvider(container);
        }

        private static void BindStoreProductProvider(DiContainer container)
        {
            var hasRegisteredProductsInStore =
                BUNDLE_IDS_WITH_REGISTERED_PRODUCTS_IN_STORE.TryGetValue(Application.platform, out var ids) &&
                ids.Any(x => x == Application.identifier);
            if (hasRegisteredProductsInStore || Application.isEditor)
            {
                container.BindInterfacesAndSelfTo<StoreProductsProvider>().AsSingle();
            }
            else
            {
                container.BindInterfacesAndSelfTo<MockStoreProductsProvider>().AsSingle();
            }
        }
    }
}