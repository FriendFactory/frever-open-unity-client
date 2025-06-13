using System;
using UnityEngine;

namespace Extensions
{
    public static class AnimatorExtensions
    {
        public static void ApplyStateTo(this Animator source, Animator dest)
        {
            dest.runtimeAnimatorController = source.runtimeAnimatorController;
            var syncState = source.GetCurrentAnimatorStateInfo(0);
            dest.Play(syncState.shortNameHash, 0, syncState.normalizedTime);
        }

        public static void AddListenerToOnAnimatorMove(this Animator animator, Action onAnimatorMove)
        {
            animator.gameObject.AddListenerToOnAnimatorMove(onAnimatorMove);
        }
        
        public static void RemoveListenerFromOnAnimatorMove(this Animator animator, Action onAnimatorMove)
        {
            animator.gameObject.RemoveListenerFromOnAnimatorMove(onAnimatorMove);
        }
    }
}