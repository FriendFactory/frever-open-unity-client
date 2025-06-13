using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{

    public class AnimatorEventsListener : StateMachineBehaviour
    {
        public event Action<AnimatorStateInfo> StateEntered;
        public event Action<AnimatorStateInfo> StateMoved;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            StateEntered?.Invoke(stateInfo);
        }

        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            StateMoved?.Invoke(stateInfo);
        }
    }
}