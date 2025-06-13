using System.Collections.Generic;
using UnityEngine;

namespace SetScripts
{
    public class PathShow : MonoBehaviour
    {
        public Color customColor;
        private List<Transform> _waypoints;

        private void OnDrawGizmos()
        {
            Gizmos.color = customColor;

            var pathNodes = GetComponentsInChildren<Transform>();
            _waypoints = new List<Transform>();

            for (int i = 0; i < pathNodes.Length; i++)
            {
                if (pathNodes[i] != transform)
                {
                    _waypoints.Add(pathNodes[i]);
                }
            }

            for (int i = 0; i < _waypoints.Count; i++)
            {
                var current = _waypoints[i].position;
                var previous = Vector3.zero;

                if (i > 0)
                {
                    previous = _waypoints[i - 1].position;
                }
                else if (_waypoints.Count > 1 && i == 0)
                {
                    previous = _waypoints[_waypoints.Count - 1].position;
                }

                Gizmos.DrawLine(previous, current);
                Gizmos.DrawSphere(current, 0.2f);
            }
        }
    }
}