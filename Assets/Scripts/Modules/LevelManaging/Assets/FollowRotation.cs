using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    internal sealed class FollowRotation: MonoBehaviour
    {
        private Transform _target;
        
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if(_target==null) return;
            
            transform.rotation = _target.rotation;
        }
    }
}