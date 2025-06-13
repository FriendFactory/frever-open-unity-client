using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoatAttack;


namespace BoatAttack
{
    public class ReflectionProbeSwitcher : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _dayTimeProbe = new List<GameObject>();
        [SerializeField]
        private List<GameObject> _nightTimeProbe = new List<GameObject>();

        [SerializeField]
        private DayNightController2 _dayNightController;

        private void Awake()
        {
            _dayNightController = GetComponentInParent<DayNightController2>();
        }

        private void Update()
        {
            switch (_dayNightController.CurrentDayStatus)
            {
                case DayStatus.Day:
                case DayStatus.Morning:
                    foreach (GameObject dayProbe in _dayTimeProbe)
                    {
                        dayProbe.SetActive(true);
                    }
                    foreach (GameObject nightProbe in _nightTimeProbe)
                    {
                        nightProbe.SetActive(false);
                    }
                    break;

                case DayStatus.Night:
                    foreach (GameObject dayProbe in _dayTimeProbe)
                    {
                        dayProbe.SetActive(false);
                    }
                    foreach (GameObject nightProbe in _nightTimeProbe)
                    {
                        nightProbe.SetActive(true);
                    }
                    break;
            }
        }
    }
}
