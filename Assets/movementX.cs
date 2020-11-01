using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementX : MonoBehaviour
{
    private Rigidbody _rigidBody;

    [SerializeField]
    private float _speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.AddForce(new Vector3(0, 0, 10), ForceMode.VelocityChange);
    }

    // Update is called once per frame



    void FixedUpdate()
    {
        
        //_rigidBody.AddForce(new Vector3(0, 0, 300),ForceMode.Impulse);
        //_rigidBody.AddForce(new Vector3(_speed * Time.fixedDeltaTime, 0, 0), ForceMode.VelocityChange);
        //_rigidBody.AddForce(new Vector3(0,-9.81f,0),ForceMode.Acceleration);
        //_rigidBody.MovePosition(_rigidBody.position + new Vector3(_speed * Time.fixedDeltaTime , 0, 0) );
        //_rigidBody.velocity = new Vector3(_speed, 0, 0);
    }
}
