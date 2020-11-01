using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSControllerCharacter2 : MonoBehaviour
{
    [SerializeField]
    private float mass = 80;

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

    private Vector3 facing = Vector3.zero;
    private Vector3 straffing = Vector3.zero;


    private Vector2 _deltaLook = Vector2.zero;
    private Vector2 _lastDeltaLook = Vector2.zero;
    private Vector2 _deltaLookSmoothDampVelocity = Vector2.zero;

    private Vector2 _deltaMove = Vector2.zero;
    private Vector2 _lastDeltaMove = Vector2.zero;
    private Vector2 _deltaMoveSmoothDampVelocity = Vector2.zero;

    [SerializeField]
    private float _walkSpeed = 3f;

    [SerializeField]
    private float _airSpeed = 1f;

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

    private Vector3 _movementVelocity = Vector3.zero;

    private bool _perfomedJump = false;

    private bool _performedFire = false;

    private bool _performedReload = false;

    [SerializeField]
    private float _jumpHeight = 1.5f;

    private bool _isOnFloor = false;

    private Vector3 _groundVelocity = Vector3.zero;

    private bool _aiming = false;

    private Camera _camera;

    [SerializeField]
    private float _cameraFOV = 70;
    [SerializeField]
    private float _cameraAimingFOV = 40;

    private float _cameraFOVTarget = 0;
    private float _cameraFOVSmoothDampVelocity = 0;

    [SerializeField]
    private float _slopeLimit = 30f; //degree

    private Vector3 _slideVelocityAccumulation = Vector3.zero;

    [SerializeField]
    private float _groundFriction = 60f;


    [SerializeField]
    private float _AirFriction = 10f;

    private Vector3 HorizontalMovement = Vector3.zero;
    private Vector3 HorizontalMovementDampVelocity = Vector3.zero;

    //private float VerticalMomentum = 0f;
    //private float _verticalMovement = 0f;

    [SerializeField]
    private float _jumpRecover = 0.5f;

    private float _jumpRecoverTimeLeft = 0;

    [SerializeField]
    private WeaponControler _weaponController;

    [SerializeField]
    private float pushForce = 5;

    private float friction = 0;

    //private Vector3 force = 0;


    private Vector3 _horizontalVelocity = Vector3.zero;

    private Vector3 _verticalVelocity = Vector3.zero;
    private Vector3 _verticalMomentum = Vector3.zero;





    [SerializeField]
    private float _slopeSpeedAffect = 0.5f;

    private float AccelerationMax = 500;


    private Vector3 momentum = Vector3.zero;

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

        friction = _AirFriction;

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

        //Split v into components u perpendicular to the wall and w parallel to it.
        Vector3 u = Vector3.Dot(collision.relativeVelocity, averageNormal) * averageNormal;
        Vector3 w = collision.relativeVelocity - u;

        PhysicMaterial otherPhysicMaterial = collision.collider.material;
        PhysicMaterial thisPhysicMaterial = _capsuleCollider.material;

        



        if (collision.rigidbody != null)
        {
            Vector3 targetVelocity = (w - u) * getBounciness(otherPhysicMaterial, thisPhysicMaterial) ;
            //Debug.Log("force" + forceToApply  );
            collision.rigidbody.velocity = targetVelocity ;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Vector3 averageContact = Vector3.zero;
        Vector3 averageNormal = Vector3.zero;

        foreach(ContactPoint contactPoint in collision.contacts)
        {
            averageContact += contactPoint.point / collision.contactCount;
            averageNormal += contactPoint.normal / collision.contactCount;
        }
        
        if (collision.rigidbody != null)
        {
            Vector3 forceToApply = mass/collision.rigidbody.mass * -collision.relativeVelocity.magnitude * pushForce * averageNormal ;
            collision.rigidbody.AddForceAtPosition(forceToApply, averageContact,ForceMode.Acceleration);


            if( Vector3.Dot(collision.relativeVelocity , averageNormal) < 0)
            {
                Vector3 forceONCharacter = collision.rigidbody.mass / mass * collision.relativeVelocity.magnitude * averageNormal;
                AddForce(forceONCharacter);
            }
                
        }


        /*Debug.Log(collision.relativeVelocity + " " + collision.impulse);

        if (collision.rigidbody != null)
        {
            Vector3 forceToApply = (mass / collision.rigidbody.mass) * ContactPoint.normal * -collision.impulse.magnitude / Time.fixedDeltaTime;
            collision.rigidbody.AddForceAtPosition(forceToApply, ContactPoint.point);
        }*/
    }

    public void AddForce(Vector3 force)
    {
        
        _horizontalVelocity += force;
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

    private bool isSliding(FloorDetection floorDetection)
    {
        float angle = Vector3.Angle(floorDetection.hitNormal, transform.up);

        return angle > _slopeLimit;
    }

    private bool isFalling(FloorDetection floorDetection)
    {
        return !floorDetection.detectGround || (_verticalVelocity.y < -0.01f && floorDetection.floorDistance > 0) || _verticalVelocity.y > 0;
    }

    private bool canJump(FloorDetection floorDetection)
    {
        return !isFalling(floorDetection) && _jumpRecoverTimeLeft <= 0;
    }

    private bool tryJump(FloorDetection floorDetection)
    {
        Debug.Log("testJump");
        if (canJump(floorDetection))
        {

            Debug.Log("testJump2");
            _jumpRecoverTimeLeft = _jumpRecover;
            float jumpVelocity = Mathf.Max(0, _groundVelocity.y) + Mathf.Sqrt(2 * _jumpHeight * -_gravity);

            if ((_verticalMomentum + jumpVelocity*transform.up).magnitude > 0)
            {
                Debug.Log("testJump3");
                _verticalMomentum += jumpVelocity * transform.up;
                _verticalVelocity = _verticalMomentum;
                return false;
            }
        }
        return false;
    }

    public Vector3 applyFriction(Vector3 current,  Vector3 target, float PerpendicularForceToSurface, float friction, float deltaTime)
    {

        float frictionAttenuation = PerpendicularForceToSurface * friction * deltaTime;

        Vector3 displacement = target - current;
        Vector3 next = Vector3.zero;

        if(displacement.magnitude < (PerpendicularForceToSurface * deltaTime ))
            next = target;
        else
            next = current + displacement.normalized * (PerpendicularForceToSurface * deltaTime )  ;

        return next;
    }



    private void computeMovement2()
    {
       

        //add gravity to momentum
        _verticalMomentum += _gravity * Time.fixedDeltaTime * transform.up;

        FloorDetection floorDetection = _floorSensor.GetFloorDetection();

        if(!isFalling(floorDetection))
        {
            Vector3 groundFacing = Vector3.ProjectOnPlane(facing, floorDetection.hitNormal).normalized;
            Vector3 groundStraffing = Vector3.ProjectOnPlane(straffing, floorDetection.hitNormal).normalized;

            if(!isSliding(floorDetection))
            {

                _verticalMomentum = Vector3.zero;

            }
            else
            {
                //_horizontalMomentum.pro
            }

        }
        else
        {   


        }

    }

    private void computeMovement()
    {
        friction = _groundFriction;

        Vector3 targetHorizontalVelocity = Vector3.zero;

        float groundCorrection = 0;

        FloorDetection floorDetection = _floorSensor.GetFloorDetection();
        float angle = Vector3.Angle(floorDetection.hitNormal, transform.up);


        bool hasJump = false;

        if (_perfomedJump)
            hasJump = tryJump(floorDetection);


        if (isFalling(floorDetection))
        {
            friction = _AirFriction;
            targetHorizontalVelocity = (_lastDeltaMove.x * straffing + _lastDeltaMove.y * facing ) * _walkSpeed;
            _verticalMomentum += _gravity * Time.fixedDeltaTime * transform.up;
            _verticalVelocity = _verticalMomentum;
        }
        else
        {

            Vector3 groundFacing = Vector3.ProjectOnPlane(facing, floorDetection.hitNormal).normalized;
            Vector3 groundStraffing = Vector3.ProjectOnPlane(straffing, floorDetection.hitNormal).normalized;

            friction = floorDetection.collider.material.dynamicFriction;

            if(!hasJump)
                groundCorrection = -floorDetection.floorDistance / Time.fixedDeltaTime;

            targetHorizontalVelocity = (_lastDeltaMove.x * groundStraffing + _lastDeltaMove.y * groundFacing) * _walkSpeed;
            _verticalVelocity = Vector3.zero;

            float slopeSpeedFactor = 1.0f;
            
            if(targetHorizontalVelocity.magnitude > 0.1f)
                slopeSpeedFactor = -Vector3.Dot(transform.up, targetHorizontalVelocity.normalized) + 1.0f;

            friction *= Vector3.Dot(transform.up, targetHorizontalVelocity/_walkSpeed) + 1.0f;

            targetHorizontalVelocity *= Mathf.Lerp(slopeSpeedFactor, 1.0f, _slopeSpeedAffect);

            if (isSliding(floorDetection))
            {

                _verticalMomentum += _gravity * Time.fixedDeltaTime * transform.up * Mathf.Max(0,(1-friction));
                //Debug.Log(1 - friction);

                Vector3 horizontalSlopeGravityMomentum = Vector3.ProjectOnPlane(_verticalMomentum, floorDetection.hitNormal);
                targetHorizontalVelocity += horizontalSlopeGravityMomentum;



                //friction *= horizontalVelocitySlopeFactor;
                //Debug.Log(friction);

                //float facingProjection = Vector3.Dot(horizontalMomentum, facing);
                //float straffingProjection = Vector3.Dot(horizontalMomentum, straffing);



                // Debug.Log(horizontalVelocitySlopeFactor);


                //Debug.Log("friction " + Vector3.Dot(transform.up, _rigidbody.velocity.normalized));

                //Debug.DrawRay(transform.position, floorDetection.hitNormal, Color.red, Time.fixedDeltaTime + 0.001f);
                //Debug.DrawRay(transform.position, _rigidbody.velocity, Color.green, Time.fixedDeltaTime + 0.001f);

                //targetHorizontalVelocity *= horizontalVelocitySlopeFactor;
                

            }
            else
            {
                _verticalMomentum = Vector3.zero;
            }
        }

        //targetHorizontalVelocity = targetHorizontalVelocity.x * straffing + targetHorizontalVelocity.z * facing;


        Vector3 perpendicularForce = mass * _gravity * transform.up;
        _horizontalVelocity = applyFriction(_horizontalVelocity, targetHorizontalVelocity, perpendicularForce.magnitude, friction, Time.fixedDeltaTime);

        //_horizontalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, _walkSpeed);


        /*HorizontalMovement = Vector3.SmoothDamp(
            HorizontalMovement,
            targetHorizontalVelocity,
            ref HorizontalMovementDampVelocity,
            0.02f,
            friction * _walkSpeed / 0.1f, // at friction = 1 maxSpeed is reach in 0.1s
            Time.fixedDeltaTime);
        */

        _rigidbody.velocity = _horizontalVelocity + _verticalVelocity + groundCorrection * transform.up;

        //_rigidbody.AddForce(targetHorizontalVelocity - _rigidbody.velocity, ForceMode.VelocityChange);

        _perfomedJump = false;

        if (_jumpRecoverTimeLeft > 0)
            _jumpRecoverTimeLeft -= Time.fixedDeltaTime;

    }


    private void FixedUpdate()
    {
        _floorSensor.Cast();
        computeMovement();
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

        facing = _head.forward;
        straffing = _head.right;

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
