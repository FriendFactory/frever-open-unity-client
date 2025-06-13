using BoatAttack;
using UnityEngine;

namespace SetScripts
{
    internal sealed class DayNightParticleController : MonoBehaviour
    {
        [SerializeField] private DayNightController2 _dayNightController;
        [SerializeField] private ParticleSystem[] _particleSystems;
        [SerializeField] private bool _enableDuringDay = false;

        private void Update()
        {
            if (_dayNightController == null) return;

            var currentDayStatus = _dayNightController.CurrentDayStatus;
            var shouldEnableParticles = _enableDuringDay ? currentDayStatus != DayStatus.Night : currentDayStatus == DayStatus.Night;
            EnableParticleSystems(shouldEnableParticles);
        }

        private void EnableParticleSystems(bool enable)
        {
            foreach (var particle in _particleSystems)
            {
                particle.gameObject.SetActive(enable);
            }
        }
    }
}