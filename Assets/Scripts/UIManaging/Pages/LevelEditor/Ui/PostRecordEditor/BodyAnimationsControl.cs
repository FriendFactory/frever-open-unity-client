using System.Linq;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class BodyAnimationsControl
    {
        private Animator[] _animators;
        private float _previousSpeed;
            
        public void SetTargets(Animator[] animators)
        {
            _animators = animators;
        }

        public void PrepareForForcingUpdate()
        {
            _previousSpeed = _animators.First().speed;
            SetSpeed(1);
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < _animators.Length; i++)
            {
                _animators[i].Update(deltaTime);
            }
        }

        public void CompleteForcingUpdate()
        {
            SetSpeed(_previousSpeed);
        }

        private void SetSpeed(float speed)
        {
            foreach (var animator in _animators)
            {
                animator.speed = speed;
            }
        }
    }
}