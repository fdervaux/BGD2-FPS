using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSControllerCharacter : MonoBehaviour
{
    [SerializeField]
    private float _mass = 80;

    private Transform _body;

    [SerializeField]
    private Transform _head = null;

    [SerializeField]
    private float _mouseSensitivity = 60f;

    private float _verticalHeadAngle = 0f;

    [SerializeField]
    private float _verticalHeadMinimuAngle = -90f;
    [SerializeField]
    private float _verticalHeadMaximumAngle = 90f;
    private float _horizontalRotationAccumulation = 0f;
    private Vector3 _facing = Vector3.zero;
    private Vector3 _straffing = Vector3.zero;


    private Vector2 _deltaLook = Vector2.zero;
    private Vector2 _lastDeltaLook = Vector2.zero;
    private Vector2 _deltaLookSmoothDampVelocity = Vector2.zero;

    private Vector2 _deltaMove = Vector2.zero;
    private Vector2 _lastDeltaMove = Vector2.zero;
    private Vector2 _deltaMoveSmoothDampVelocity = Vector2.zero;

    [SerializeField, Range(0,10)]
    private float _moveSpeed = 3f;

    [SerializeField, Range(0,100)]
    private float _maxMoveAcceleration = 50;


    private Rigidbody _rigidbody;

    private Animator _animator;

    [SerializeField]
    private FloorSensor _floorSensor;

    private CapsuleCollider _capsuleCollider;

    [SerializeField]
    private float _step = 0.3f;

    [SerializeField]
    private float _height = 2.0f;

    [SerializeField]
    private float _radius = 0.5f;

    [SerializeField]
    private float _gravity = -9.81f;


    private bool _perfomedJump = false;

    private bool _performedFire = false;

    private bool _performedReload = false;

    [SerializeField]
    private float _jumpHeight = 1.5f;

    private Vector3 _groundVelocity = Vector3.zero;

    private bool _aiming = false;



    private Camera _camera;

    [SerializeField]
    private float _cameraFOV = 70;
    [SerializeField]
    private float _cameraAimingFOV = 40;

    private float _cameraFOVTarget = 0;
    private float _cameraFOVSmoothDampVelocity = 0;

    //[SerializeField]
    //private float _slopeLimit = 30f; //degree

    //[SerializeField, Range(0,1)]
    //private float _AirControl = 1f;

    [SerializeField, Range(0, 1)]
    private float _AirFriction = 1;

    [SerializeField, Range(0, 1)]
    private float _drag = 0.01f;


    [SerializeField]
    private float _jumpRecover = 0.5f;

    private float _jumpRecoverTimeLeft = 0;

    [SerializeField]
    private WeaponControler _weaponController;

    [SerializeField]
    private float _pushForce = 5;




    [SerializeField, Range(0,1)]
    private float _slopeSpeedAffect = 0.5f;

    private Vector3 _momentum = Vector3.zero;
    private Vector3 _horizontalMove = Vector3.zero;
    private Vector3 _groundCorrection = Vector3.zero;


    private bool _isOnGround = false;
    private bool _isSliding = false;
    private FloorDetection _floorDetection;

    private float _slopeAngle = 0; //rad

    private Vector3 _lastMomentum = Vector3.zero; //use for collision 





    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _body = transform;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _camera = GetComponentInChildren<Camera>();

        _cameraFOVTarget = _cameraFOV;
        calibrateSensor();
        calibrateCollider();
    }

    public void OnMove(InputValue value)
    {
        _deltaMove = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _deltaLook = value.Get<Vector2>();
    }

    public void OnJump()
    {
        _perfomedJump = true;
    }

    public void OnFire()
    {
        _performedFire = true;
    }
    
    public void OnReload()
    {
        _performedReload = true;
    }

    public void OnAiming(InputValue value)
    {
        _aiming = value.Get<float>() > 0.5f ? true : false;
    }


    private float getBounciness(PhysicMaterial material1, PhysicMaterial material2)
    {
        if(material1.bounceCombine == PhysicMaterialCombine.Average)
            return (material1.bounciness + material2.bounciness) / 2;
        
        if(material1.bounceCombine == PhysicMaterialCombine.Maximum)
            return Mathf.Max(material1.bounciness, material2.bounciness);

        if (material1.bounceCombine == PhysicMaterialCombine.Minimum)
            return Mathf.Min(material1.bounciness, material2.bounciness);

        if (material1.bounceCombine == PhysicMaterialCombine.Multiply)
            return material1.bounciness * material2.bounciness;

        return 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 averageContact = Vector3.zero;
        Vector3 averageNormal = Vector3.zero;

        foreach (ContactPoint contactPoint in collision.contacts)
        {
            averageContact += contactPoint.point / collision.contactCount;
            averageNormal += contactPoint.normal / collision.contactCount;
        }

        if (collision.rigidbody != null)
        {

            Vector3 relativeVelocity =  _lastMomentum - collision.rigidbody.velocity;
            averageNormal *= -1; 

            //Split v into components u perpendicular to the wall and w parallel to it.
            Vector3 u = Vector3.Project(relativeVelocity, averageNormal);
            Vector3 w = relativeVelocity - u;

            PhysicMaterial otherPhysicMaterial = collision.collider.material;
            PhysicMaterial thisPhysicMaterial = _capsuleCollider.material;

            Debug.Log("velocity " + collision.impulse + " normal " + averageNormal);

            Debug.DrawRay(averageContact, relativeVelocity, Color.yellow, 10);
            Debug.DrawRay(averageContact, averageNormal, Color.cyan, 10);

            Debug.Log(Vector3.Project(_lastMomentum, averageNormal).magnitude * _mass);
            Debug.Log(Vector3.Project(collision.rigidbody.velocity, averageNormal).magnitude * collision.rigidbody.mass);


            bool recieveColision = Vector3.Project(_lastMomentum, averageNormal).magnitude * _mass < Vector3.Project(collision.rigidbody.velocity, averageNormal).magnitude * collision.rigidbody.mass;


            float bFactor = getBounciness(otherPhysicMaterial, thisPhysicMaterial);
           // collision.rigidbody.AddForce(collision.rigidbody.velocity * (1 - bFactor) + (u - w) * bFactor * _pushForce, ForceMode.VelocityChange);


            
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Vector3 averageContact = Vector3.zero;
        Vector3 averageNormal = Vector3.zero;

        foreach (ContactPoint contactPoint in collision.contacts)
        {
            averageContact += contactPoint.point / collision.contactCount;
            averageNormal += contactPoint.normal / collision.contactCount;
        }

        if (collision.rigidbody != null)
        {
            Vector3 relativeVelocity = _lastMomentum - collision.rigidbody.velocity;
            averageNormal *= -1;

            Debug.DrawRay(averageContact, collision.impulse, Color.yellow, Time.fixedDeltaTime * 1.01f);
            Debug.DrawRay(averageContact, averageNormal, Color.cyan, Time.fixedDeltaTime *1.0f);

            Debug.Log(relativeVelocity);

            Vector3 forceToApply = _mass * _moveSpeed * Vector3.Project(_horizontalMove,averageNormal) * _pushForce;
            //collision.rigidbody.AddForceAtPosition(forceToApply, averageContact, ForceMode.Force);
        }
    }

    public void AddForce(Vector3 force)
    {
        Debug.Log("force add");
        _rigidbody.AddForce(force, ForceMode.VelocityChange);
    }

    public void calibrateSensor()
    {
        _floorSensor.init(transform);
        //add 0.01 to avoid sensor on ground 
        _floorSensor.SetOffset(new Vector3(0, _step + 0.01f, 0));
        _floorSensor.SetCastLength(_step * 3);
    }

    public void calibrateCollider()
    {
        _capsuleCollider.height = _height - _step;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.height / 2 + _step, 0);
        _capsuleCollider.radius = _radius;
    }



    private void handleGround()
    {
        _floorSensor.Cast();
        _floorDetection = _floorSensor.GetFloorDetection(); 

        _isOnGround = false;
        _isSliding = false;

        if (_floorDetection.detectGround)
        {
            float YVelocity = Vector3.Project(_momentum, _floorDetection.hitNormal).magnitude;
            _isOnGround = YVelocity < 0.01f || _floorDetection.floorDistance < 0;
            _slopeAngle = Vector3.Angle(_floorDetection.hitNormal, transform.up) * Mathf.Deg2Rad;
            float staticFriction = _floorDetection.collider.material.staticFriction;
            _isSliding = _isOnGround && staticFriction < Mathf.Tan(_slopeAngle);
        }

        _groundCorrection = Vector3.zero;

        if( _isOnGround )
            _groundCorrection = (-_floorDetection.floorDistance / Time.fixedDeltaTime) * transform.up;


        //Debug.Log("isONGround " + _isOnGround + " isSliding " + _isSliding + " slopeAnge " + _slopeAngle + " groundCorrection " + _groundCorrection);
    }

    private void computeMomentum()
    {
        Vector3 _verticalMomentum = Vector3.zero;
        Vector3 _horizontalMomentum = Vector3.zero;

        
        //Split momentum into vertical and horizontal components;
        if ( _isOnGround)
            _verticalMomentum = Vector3.Project(_momentum, _floorDetection.hitNormal);
        else
            _verticalMomentum = Vector3.Project(_momentum, transform.up);

        _horizontalMomentum = _momentum - _verticalMomentum;

        float frictionAttenuation = 0;

        if(_isOnGround)
        {
            if (!_isSliding)
            {
                _verticalMomentum = Vector3.zero;
                _momentum = _horizontalMomentum + _verticalMomentum;
            }
            else
            {
                _verticalMomentum = -_maxMoveAcceleration * Time.fixedDeltaTime * transform.up;
                _momentum = _horizontalMomentum + _verticalMomentum;
                _momentum = Vector3.ProjectOnPlane(_momentum, _floorDetection.hitNormal);
            }

            float dynamicFriction = _floorDetection.collider.material.dynamicFriction;
            //Debug.Log("dynamicfriction " + dynamicFriction + " slopeAngle " + _slopeAngle );
            frictionAttenuation = _maxMoveAcceleration * dynamicFriction * Mathf.Cos(_slopeAngle);
            _momentum = Vector3.MoveTowards(_momentum, _horizontalMove*_moveSpeed, frictionAttenuation * Time.fixedDeltaTime);
        }
        else
        {
            _verticalMomentum +=  _gravity * Time.fixedDeltaTime * transform.up;

            float airFriction = _maxMoveAcceleration * _AirFriction;
            _horizontalMomentum = Vector3.MoveTowards(_horizontalMomentum, _horizontalMove*_moveSpeed, airFriction * Time.fixedDeltaTime);

            _momentum = _horizontalMomentum + _verticalMomentum;
            _momentum *= Mathf.Max(0,1-_drag * Time.fixedDeltaTime ) ;

        }
        
        
        
    }

    private void tryJump()
    {
        if(_perfomedJump && _isOnGround && !_isSliding)
        {
            float jumpVelocity = Mathf.Max(0, _groundVelocity.y) + Mathf.Sqrt(2 * _jumpHeight * -_gravity);

            if(Vector3.Dot(_momentum,transform.up) < jumpVelocity )
            {
                _momentum = _momentum - Vector3.Project(_momentum,transform.up);
                _momentum += transform.up * jumpVelocity;
            }

            _isOnGround = false;
        }
    }    

    private void computeMovement()
    {
        Vector3 groundNormal = transform.up;
        float friction = 1;

        if(_isOnGround)
        {
            groundNormal = _floorDetection.hitNormal;
            friction = _floorDetection.collider.material.dynamicFriction;
        }

        Vector3 groundFacingAxis = Vector3.ProjectOnPlane(_facing, groundNormal).normalized;
        Vector3 groundStraffingAxis = Vector3.ProjectOnPlane(_straffing, groundNormal).normalized;

        Vector3 targetVelocity = _lastDeltaMove.x * groundStraffingAxis + _lastDeltaMove.y * groundFacingAxis;

        _horizontalMove = targetVelocity;
    }


    private void FixedUpdate()
    {   
        
        _momentum = _rigidbody.velocity - _groundCorrection;

        if(_isOnGround)
            _momentum = _momentum - Vector3.Project(_momentum, _floorDetection.hitNormal);

        handleGround();
        computeMovement();
        computeMomentum();
        tryJump();

        _rigidbody.velocity = _momentum + _groundCorrection;
        _lastMomentum = _momentum;

        _perfomedJump = false;
    }

    private void HandleAiming()
    {
        _cameraFOVTarget = _aiming ? _cameraAimingFOV : _cameraFOV;
        _camera.fieldOfView = Mathf.SmoothDamp(_camera.fieldOfView, _cameraFOVTarget, ref _cameraFOVSmoothDampVelocity, 0.04f);
        _animator.SetBool("aiming", _aiming);
    }

    private void HandleCameraLook()
    {
        _lastDeltaLook = Vector2.SmoothDamp(_lastDeltaLook, _deltaLook, ref _deltaLookSmoothDampVelocity, 0.04f);
        _horizontalRotationAccumulation += _lastDeltaLook.x * Time.deltaTime * _mouseSensitivity;

        _verticalHeadAngle = Mathf.Clamp(
            _verticalHeadAngle + _lastDeltaLook.y * Time.deltaTime * _mouseSensitivity,
            _verticalHeadMinimuAngle,
            _verticalHeadMaximumAngle);

        _head.localRotation = Quaternion.Euler(new Vector3(0, _horizontalRotationAccumulation, 0));

        _facing = _head.forward;
        _straffing = _head.right;

        _head.localRotation = Quaternion.Euler(new Vector3(-_verticalHeadAngle, _horizontalRotationAccumulation, 0));
    }

    private void HandleMovement()
    {
        _animator.SetFloat("speed", _lastDeltaMove.magnitude);
        _lastDeltaMove = Vector2.SmoothDamp(_lastDeltaMove, _deltaMove, ref _deltaMoveSmoothDampVelocity, 0.04f);
    }

    private void HandleFire()
    {
        if (_performedFire)
        {
            _animator.SetTrigger("fire");
            _weaponController.decreaseAmmo();
            _performedFire = false;
        }
    }

    private void HandleReload()
    {
        if (_performedReload)
        {
            _animator.SetTrigger("reload");
            _weaponController.reloadAmmo();
            _performedReload = false;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleAiming();
        HandleMovement();
        HandleCameraLook();
        HandleFire();
        HandleReload();
    }

    void Start()
    {
        StartCoroutine(LateFixedUpdate());
    }

    IEnumerator LateFixedUpdate()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            _groundVelocity = Vector3.zero;

            FloorDetection floorDetection = _floorSensor.GetFloorDetection();

            if (floorDetection.detectGround)
            {
                Rigidbody groundMovePlateforme = floorDetection.collider.transform.GetComponentInParent<Rigidbody>();

                if (groundMovePlateforme != null)
                {
                    
                    Vector3 offset = _rigidbody.position - groundMovePlateforme.position;
                    Vector3 rotateOffset = Quaternion.Euler(groundMovePlateforme.angularVelocity*Mathf.Rad2Deg * Time.fixedDeltaTime) * offset;

                    Debug.Log("offset : " + offset + " => rotateOffset : " + rotateOffset + " from AngularVelocity " + groundMovePlateforme.angularVelocity);

                    _rigidbody.MovePosition(groundMovePlateforme.position + rotateOffset + groundMovePlateforme.velocity * Time.fixedDeltaTime);
                    _rigidbody.MoveRotation(Quaternion.Euler(groundMovePlateforme.angularVelocity.y * transform.up * Mathf.Rad2Deg * Time.fixedDeltaTime) * _rigidbody.rotation);

                    _groundVelocity = groundMovePlateforme.velocity;
                }
            }
        }
    }
}
