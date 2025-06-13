using ModestTree;
using UnityEngine;
using Zenject;

namespace ThirdPackagesExtends.Zenject
{
    /// <summary>
    /// Used similar to ZenjectAutoInjector component. Difference is only - we can invoke injection even on disabled prefab.
    /// It is usefull during UI generation together with Enhanced scroller package, which make instantiated prefabs not active.
    /// To force inject on not active object you should invoke Inject() method
    /// </summary>
    internal class DependencyInjector : MonoBehaviour
    {
        [SerializeField]
        ContainerSources _containerSource = ContainerSources.SearchHierarchy;

        bool _hasInjected;

        public ContainerSources ContainerSource
        {
            get { return _containerSource; }
            set { _containerSource = value; }
        }

        // Make sure they don't cause injection to happen twice
        [Inject]
        public void Construct()
        {
            if (!_hasInjected)
            {
                throw Assert.CreateException(
                    "ZenAutoInjecter was injected!  Do not use ZenAutoInjecter for objects that are instantiated through zenject or which exist in the initial scene hierarchy");
            }
        }

        public void Awake()
        {
            if(_hasInjected)
                return;
            
            Inject();
        }

        public void Inject()
        {
            _hasInjected = true;
            LookupContainer().InjectGameObject(gameObject);
        }
        
        DiContainer LookupContainer()
        {
            if (_containerSource == ContainerSources.ProjectContext)
            {
                return ProjectContext.Instance.Container;
            }

            if (_containerSource == ContainerSources.SceneContext)
            {
                return GetContainerForCurrentScene();
            }

            Assert.IsEqual(_containerSource, ContainerSources.SearchHierarchy);

            var parentContext = transform.GetComponentInParent<Context>();

            if (parentContext != null)
            {
                return parentContext.Container;
            }

            return GetContainerForCurrentScene();
        }

        DiContainer GetContainerForCurrentScene()
        {
            return ProjectContext.Instance.Container.Resolve<SceneContextRegistry>()
                .GetContainerForScene(gameObject.scene);
        }

        public enum ContainerSources
        {
            SceneContext,
            ProjectContext,
            SearchHierarchy
        }
    }
}
