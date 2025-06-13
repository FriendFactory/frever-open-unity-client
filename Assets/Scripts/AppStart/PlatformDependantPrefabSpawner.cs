using UnityEngine;

namespace AppStart
{
    internal sealed class PlatformDependantPrefabSpawner: MonoBehaviour
    {
        [SerializeField] private RuntimePlatform _targetPlatform = RuntimePlatform.Android;
        [SerializeField] private GameObject _prefab;

        private void Start()
        {
            if (Application.platform != _targetPlatform) return;
            Instantiate(_prefab);
        }
    }
}