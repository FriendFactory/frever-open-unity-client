using BoatAttack;
using UnityEngine;

[ExecuteInEditMode]
public class FogColorDriver : MonoBehaviour
{
    [SerializeField] private HeightFogGlobal _globalFog;
    [SerializeField] private DayNightController2 _dayNightController;

    private Color _currentColor;
    private Color _oldColor;
    
    
    void LateUpdate()
    {
        _currentColor = _dayNightController.FogColour.Evaluate(_dayNightController.Time);
        if (_oldColor != _currentColor)
        {
            _globalFog.fogColor = _dayNightController.FogColour.Evaluate(_dayNightController.Time);
            _oldColor = _currentColor;
        }
    }
}