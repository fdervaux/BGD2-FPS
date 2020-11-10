using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootToGoal : AbstractAIBehaviour
{
    public override int GetBehaviourHash()
    {
        return BehaviourHashes.SHOOT_TO_GOAL;
    }

    public override void onEnterState()
    {
        agentController.ShootOnGoal();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        agentController._direction = Vector3.zero;
    }
}
