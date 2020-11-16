using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunToBall : AbstractAIBehaviour
{

    public override int GetBehaviourHash()
    {
        return BehaviourHashes.RUN_TO_BALL;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("RunToBall" + agentController.ball.position);

        Vector3 ballToME = agentController.ball.position - transform.position;
        float Distance = ballToME.magnitude;
        Vector3 direction = ballToME.normalized;

        agentController._direction = direction;
    }
}
