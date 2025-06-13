using UnityEngine;

namespace ThirdPackagesExtends.Zenject
{
    public static class ZenjectExtensions
    {
        public static void InjectDependenciesIfNeeded(this GameObject gameObject)
        {
            var injector = gameObject.GetComponent<DependencyInjector>();
            if(injector == null)
                return;
            
            injector.Inject();
        }
    }
}