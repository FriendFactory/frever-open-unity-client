using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class ZenProjectContextInjecter : MonoBehaviour
    {
        private bool _hasInjected;

        // Make sure they don't cause injection to happen twice
        [Inject]
        public void Construct()
        {
            if (!_hasInjected)
            {
                throw Assert.CreateException(
                    "ZenAutoInjecter was injected! " +
                    "Do not use ZenAutoInjecter for objects that are instantiated through zenject " +
                    "or which exist in the initial scene hierarchy");
            }
        }

        public void Awake()
        {
            if (_hasInjected) return;
            
            _hasInjected = true;
            ProjectContext.Instance.Container.InjectGameObject(gameObject);
        }
    }
}