using System.Collections.Generic;
using UnityEngine;

namespace SetScripts
{
    public class PathFollowBalloon : MonoBehaviour
    {
        public Transform path ;
        List<Transform> waypoints;
        [Range(0.0f, 3f)]
        public float _velocity = 0.05f;
        private int currentNode;
        
        private void Start()
        {
            Transform[] pathNodes = path.GetComponentsInChildren<Transform>();
            waypoints = new List<Transform>();

            for (int i = 0 ; i<pathNodes.Length ; i++){

                if (pathNodes[i] != path.transform){

                    waypoints.Add(pathNodes[i]);
                }
            }
        }
        private void FixedUpdate() 
        {
            Move();
            ProximityCheck();
        }

        private void ProximityCheck()
        {
            if (Vector3.Distance(transform.position, waypoints[currentNode].position) < 1f)
            {
                if(currentNode == (waypoints.Count-1))
                {
                    currentNode = 0;
                }
                else
                {
                    currentNode++;
                }
            }
        }
        private void Move()
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentNode].position, _velocity);
        }
    }
}
