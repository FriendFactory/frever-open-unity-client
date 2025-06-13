using UnityEngine;

namespace Modules.LevelManaging.Assets.AssetHelpers
{
    internal sealed class ParticleSystemComponent : IVfxComponent
    {
        private readonly ParticleSystem _particleSystem;

        public ParticleSystemComponent(ParticleSystem particleSystem)
        {
            _particleSystem = particleSystem;
            _particleSystem.useAutoRandomSeed = false;
        }

        public void Simulate(float time)
        {
            _particleSystem.Simulate(time, true, true);
        }

        public void Play()
        {
            _particleSystem.Play();
        }

        public void Resume()
        {
            _particleSystem.Play();
        }

        public void Pause()
        {
            _particleSystem.Pause();
        }

        public void Stop()
        {
            _particleSystem.Stop();
        }
    }
}
