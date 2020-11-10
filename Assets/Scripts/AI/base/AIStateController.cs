using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is meant to be plugged in the A.I. Animator.
 * 
 * It's only role is to monitor the state transitions inside the Animator.
 */
public class AIStateController : StateMachineBehaviour
{

    /**
     * Notify the A.I controller when the animator enters a new state.
     */
    override public void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        animator.GetComponent<AIBehaviourController>().SetBehaviour(info.shortNameHash);
        animator.Update(0f);
    }
}