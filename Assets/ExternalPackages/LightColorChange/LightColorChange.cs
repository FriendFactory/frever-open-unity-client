using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BoatAttack
{

    public class LightColorChange : MonoBehaviour
    {

        [SerializeField]
        DayNightController2 dayNightController;
        Light thisLight;
        [SerializeField]
        Light directionalLight;
        [SerializeField]
        float timePositive;
        [SerializeField]
        float timeNegative;


        void Awake()
        {
            thisLight = GetComponent<Light>();

        }

        private void Update()
        {
            thisLight.enabled = (dayNightController.Time > timeNegative) && (dayNightController.Time < timePositive);
            thisLight.color = directionalLight.color;
            thisLight.transform.rotation = directionalLight.transform.rotation;
        }
    }


}