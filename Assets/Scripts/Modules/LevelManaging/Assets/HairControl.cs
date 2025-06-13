using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.FreverUMA;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    internal sealed class HairControl
    {
        private int _moveCount;
        private DynamicBone _dynamicBone;
        private AnimatorEventsListener _animatorEventsListener;
        private Animator _animator;
        private GameObject _avatarGameObject;
        
        public void SetupHairPhysicsBones(GameObject spineBoneGameObject, PhysicsSettings physicsSettings)
        {
            _dynamicBone = spineBoneGameObject?.GetComponentInChildren<DynamicBone>();
            if (_dynamicBone != null)
            {
                _dynamicBone.ApplyPhysicsSettings(physicsSettings);
            }
        }

        public void ResetHairPosition()
        {
            if (_dynamicBone is null) return;
            _dynamicBone.ResetParticlesPosition();
        }
        
        public void SetupHairPhysics(GameObject avatarGameObject, Animator animator)
        {
            _animator = animator;
            _avatarGameObject = avatarGameObject;
            if (avatarGameObject.activeInHierarchy)
            {
                PrepareHairPhysics();
            }
            else
            {
                avatarGameObject.AddListenerToEnabledEvent(OnAvatarEnabled);
            }
        }

        private void PrepareHairPhysics()
        {
            if (_animator == null)
            {
                return;
            }
            _animatorEventsListener = _animator.GetBehaviour<AnimatorEventsListener>();
            if (_animatorEventsListener is null)
            {
                Debug.LogWarning($"No Animator Events listener on Avatar {_animator.name} animator");
                return;
            }

            _animatorEventsListener.StateEntered += (stateInfo) =>
            {
                ChangePhysicsState(false);
                _animatorEventsListener.StateMoved += OnSecondCallOnMove;
            };
        }
        
        private void OnSecondCallOnMove(AnimatorStateInfo stateInfo)
        {
            if (_moveCount++ < 2) return;
            _animatorEventsListener.StateMoved -= OnSecondCallOnMove;
            ChangePhysicsState(true);
            _moveCount = 0;
        }
        
        private void ChangePhysicsState(bool enable)
        {
            if (_dynamicBone is null) return;
            _dynamicBone.enabled = enable;
        }

        private void OnAvatarEnabled()
        {
            _avatarGameObject.RemoveListenerFromEnabledEvent(OnAvatarEnabled);
            PrepareHairPhysics();
        }
    }
}