using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public Vector3 _direction = Vector3.zero;
    public float speed = 10;
    private Rigidbody _rigidBody;

    public float shootForce = 20;

    public Transform goal;
    public Transform ball;

    [SerializeField]
    private Animator _AiAnimator;


    public void ShootOnGoal()
    {
        if( (ball.position - transform.position).magnitude < 4 )
        {
            Vector3 ballToGall = goal.position - ball.position;
            Vector3 direction = ballToGall.normalized;

            ball.GetComponent<Rigidbody>().AddForce(direction * shootForce , ForceMode.VelocityChange);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update() {
        Vector3 ballToME = ball.position - transform.position;
        float distanceToBall = ballToME.magnitude;
        _AiAnimator.SetFloat("distanceToBall", distanceToBall);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rigidBody.velocity = _direction * speed;


    }
}
