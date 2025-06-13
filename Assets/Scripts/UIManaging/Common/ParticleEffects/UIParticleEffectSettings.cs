using UnityEngine;

namespace UIManaging.Common.ParticleEffects
{
    [CreateAssetMenu(menuName = "Friend Factory/UI/Particle Effects/ParticleEffect Settings")]
    public sealed class UIParticleEffectSettings: ScriptableObject
    {
        [SerializeField] private ParticleEffectType _type;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _despawnDelay = 3f;

        public ParticleEffectType Type => _type;
        public GameObject Prefab => _prefab;
        public float DespawnDelay => _despawnDelay;

        public UIParticleEffect Create()
        {
            var instance = Instantiate(_prefab);
            instance.SetActive(false);

            return instance.GetComponent<UIParticleEffect>();
        }
        
        public void OnGet(UIParticleEffect vfx) => vfx.gameObject.SetActive(true);
        public void OnRelease(UIParticleEffect vfx) => vfx.gameObject.SetActive(false);
        public void OnDestroyPoolObject(UIParticleEffect vfx) => Destroy(vfx.gameObject);
    }
}