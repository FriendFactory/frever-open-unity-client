using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoatAttack
{

    public class StreetLightController : MonoBehaviour
    {
        [SerializeField] DayNightController2 dayNightController;
        [SerializeField] Material _streetLight;
        [SerializeField] Light directionalLight;
        [SerializeField] float timePositive;
        [SerializeField] float timeNegative;
        
        Color _originalColor;

        private void Awake()
        {
            _originalColor = _streetLight.color;
        }

        private void Update()
        {
            if ((dayNightController.Time > timeNegative) && (dayNightController.Time < timePositive))
            {
                _streetLight.color = new Color(0, 0, 0);
                _streetLight.DisableKeyword("_EMISSION");
            }
            else
            {
                _streetLight.color = _originalColor;
                _streetLight.EnableKeyword("_EMISSION");
            }
        }
    }
}