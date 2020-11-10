using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourHashes
{
    // Behaviour used so the character does nothing at all.
    static public readonly int RUN_TO_BALL = Animator.StringToHash("RunToBall");

    // When the character is moving around without purpose
    static public readonly int SHOOT_TO_GOAL = Animator.StringToHash("ShootToGoal");

}