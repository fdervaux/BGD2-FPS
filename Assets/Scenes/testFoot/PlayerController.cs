using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public Vector3 _direction = Vector3.zero;
    public float speed = 10;
    public float shootForce = 20;
    private Rigidbody _rigidBody;
    public Transform ball;


    public void OnShoot()
    {
        if((ball.position - transform.position).magnitude < 6)
        {
            ball.GetComponent<Rigidbody>().AddForce((ball.position - transform.position).normalized * shootForce, ForceMode.VelocityChange);
        }
    }


    public void OnMove(InputValue value)
    {
        
        Vector2 playerMove = value.Get<Vector2>();
        _direction = new Vector3(playerMove.x, 0, playerMove.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rigidBody.velocity = _direction * speed;
    }
}