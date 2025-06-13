using System.Collections.Generic;
using StansAssets.Foundation.Patterns;
using UnityEngine;
using UnityEngine.Pool;

namespace UIManaging.Common.ParticleEffects
{
    public sealed class UIParticleEffectPool: MonoSingleton<UIParticleEffectPool>
    {
        [SerializeField] private bool _collectionCheck = true;
        [SerializeField] private int _defaultCapacity = 2;
        [SerializeField] private int _maxPoolSize = 4;
        
        readonly Dictionary<ParticleEffectType, IObjectPool<UIParticleEffect>> _pools = new();
        
        public static UIParticleEffect Spawn(UIParticleEffectSettings settings) => Instance.GetPoolFor(settings)?.Get();
        public static void ReturnToPool(UIParticleEffect vfx) => Instance.GetPoolFor(vfx.Settings)?.Release(vfx);

        IObjectPool<UIParticleEffect> GetPoolFor(UIParticleEffectSettings settings)
        {
            if (_pools.TryGetValue(settings.Type, out var pool)) return pool;

            pool = new UnityEngine.Pool.ObjectPool<UIParticleEffect>(
                settings.Create,
                settings.OnGet,
                OnRelease,
                settings.OnDestroyPoolObject,
                _collectionCheck,
                _defaultCapacity,
                _maxPoolSize
            );
            
            _pools.Add(settings.Type, pool);
            
            return pool;
        }

        private void OnRelease(UIParticleEffect vfx)
        {
            vfx.Attach(transform, Vector3.zero);
            
            vfx.Settings.OnRelease(vfx);
        }
    }
}