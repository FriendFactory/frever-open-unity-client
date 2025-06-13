using System.Collections.Generic;
using UnityEngine;

namespace SetScripts
{
    public class PathFollow : MonoBehaviour
    {
        public Transform path ;
        List<Transform> _waypoints;
        private int _currentNode;
        
        private void Start()
        {
            var pathNodes = path.GetComponentsInChildren<Transform>();
            _waypoints = new List<Transform>();

            for (int i = 0 ; i<pathNodes.Length ; i++)
            {
                if (pathNodes[i] != path.transform)
                {
                    _waypoints.Add(pathNodes[i]);
                }
            }
        }
        private void FixedUpdate() 
        {
            CalculateAngle();
            ProximityCheck();
        }

        private void CalculateAngle()
        {
            Vector3 difference = transform.InverseTransformPoint(_waypoints[_currentNode].position);
            float degree = (difference.x / difference.magnitude);
        }
        
        private void ProximityCheck()
        {
            if (Vector3.Distance(transform.position, _waypoints[_currentNode].position) < 5.0f)
            {
                if(_currentNode == (_waypoints.Count-1))
                {
                    _currentNode = 0;
                }
                else
                {
                    _currentNode++;
                }
            }
        }
    }
}
