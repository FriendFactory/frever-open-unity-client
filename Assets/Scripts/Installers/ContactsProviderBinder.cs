using Modules.Contacts;
using Zenject;

namespace Installers
{
    internal static class ContactsProviderBinder
    {
        public static void BindContactsProvider(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ContactsProvider>().AsSingle();
        }
    }
}