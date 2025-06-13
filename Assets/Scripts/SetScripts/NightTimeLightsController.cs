using BoatAttack;
using UnityEngine;

internal sealed class NightTimeLightsController : MonoBehaviour
{
    [SerializeField] private DayNightController2 _dayNightController;
    [SerializeField] private Light[] _lights;

    private void Update()
    {
        if (_dayNightController == null) return;
        
        var turnOnLights = _dayNightController.CurrentDayStatus == DayStatus.Night;
        EnableLights(turnOnLights);
    }

    private void EnableLights(bool enable)
    {
        foreach (var light in _lights)
        {
            light.enabled = enable;
        }
    }
}