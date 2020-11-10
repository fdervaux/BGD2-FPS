using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * The AIBehaviourController is meant to link the A.I. Animator with the corresponding Behaviours.
 */
public class AIBehaviourController : MonoBehaviour
{

    /**
     * Contains the available Behaviours.
     * 
     * The key of a Behaviour is the value returned by its GetBehaviourHash method.
     */
    protected Dictionary<int, AbstractAIBehaviour> behaviours = new Dictionary<int, AbstractAIBehaviour>();

    // The A.I. Animator
    private Animator stateMachine;
    // The Behaviour being currently executed.
    private AbstractAIBehaviour currentBehaviour;


    // Triggers that must exist in the A.I. Animator.
    public static readonly int BEHAVIOUR_ENDED = Animator.StringToHash("behaviour_ended");
    public static readonly int BEHAVIOUR_ERROR = Animator.StringToHash("behaviour_error");

    /**
     * Forces a behaviour, interrupting the one being currently executed.
     */
    public void SetBehaviour(int behaviorHash)
    {
        // Safely disable the current behaviour.
        if (currentBehaviour)
        {
            currentBehaviour.enabled = false;
            currentBehaviour.onExitState();
        }
            
        try
        {
            // Start the new behaviour.
            currentBehaviour = behaviours[behaviorHash];
            currentBehaviour.enabled = true;
            currentBehaviour.onEnterState();
        }
        catch (KeyNotFoundException)
        {
            currentBehaviour = null;
        }
    }

    void Awake()
    {
        stateMachine = GetComponent<Animator>();

        // For each child.
        foreach (AbstractAIBehaviour behaviour in GetComponentsInChildren<AbstractAIBehaviour>())
        {
            // Register it.
            behaviours.Add(behaviour.GetBehaviourHash(), behaviour);
        }
    }

}