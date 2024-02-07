using UnityEngine;
using Rewired;
using System.Collections;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public partial class CharacterController : InputListener
{

    private Transform _cachedTransform;
    public Transform cachedTransform
    {
        get
        {
            if (!_cachedTransform)
                _cachedTransform = transform;

            return _cachedTransform;
        }
    }

    private CapsuleCollider _capsuleCollider;
    public CapsuleCollider capsuleCollider
    {
        get
        {
            if (!_capsuleCollider)
                _capsuleCollider = GetComponent<CapsuleCollider>();

            return _capsuleCollider;
        }
    }

    private Rigidbody _rigid;
    public Rigidbody rigid
    {
        get
        {
            if (!_rigid)
                _rigid = GetComponent<Rigidbody>();

            return _rigid;
        }
    }

    [Header("PLAYER")]
    [Space(10)]
    [SerializeField] private int _playerID = 0;
    public int playerID { get { return _playerID; } }
    private Rewired.Player player;

    [Header("COMPONENTS")]
    [Space(10)]
    [SerializeField] private CharacterBehaviour _characterBehaviour;
    public CharacterBehaviour characterBehaviour { get { return _characterBehaviour; } }

    [Header("INPUTS")]
    [Space(10)]
    [SerializeField] private Vector2 _leftStickAxis;
    public Vector2 leftStickAxis { get { return _leftStickAxis; } }
    [SerializeField][Range(0f, 1f)] private float leftStickMagnitude;
    [Space]
    [SerializeField][Range(0f, 1f)] private float _leftStickDeadZone;
    public float leftStickDeadZone { get { return _leftStickDeadZone; } }
    [Space]
    [SerializeField] private Vector2 _leftStickAxisLerped;
    public Vector2 leftStickAxisLerped { get { return _leftStickAxisLerped; } }
    [SerializeField][Range(0f, 1f)] private float leftStickMagnitudeLerped;
    [Space]
    [SerializeField] private bool buttonA;
    [SerializeField] private bool buttonRB;

    [Header("MOTION")]
    [Space(10)]
    [SerializeField] private bool _isMoving;
    public bool isMoving { get { return _isMoving; } }
    [Space]
    [SerializeField] private float _minSpeed = 1f;
    public float minSpeed { get { return _minSpeed; } }
    [SerializeField] private float _maxSpeed = 8f;
    public float maxSpeed { get { return _maxSpeed; } }
    [SerializeField] private float currentSpeed;
    [Space]
    [SerializeField] private float speedLerpRate = 10f;
    [Space]
    [SerializeField][Range(0f, 1f)] private float _runSpeedThresold = 0.5f;
    public float runSpeedThresold { get { return _runSpeedThresold; } }
    [Space]
    [SerializeField][Range(0f, 1f)] private float _speedLerp;
    public float speedLerp { get { return _speedLerp; } }
    [Space]
    [SerializeField][Range(-1, 1)] private int _leftRight = 1;
    public int leftRight { get { return _leftRight; } }
    [Space]
    [SerializeField] private float uTurnDelay = 0.2f;
    [SerializeField] private bool _uTurn = false;
    public bool uTurn { get { return _uTurn; } }

    [Header("PHYSICS")]
    [Space(10)]
    [SerializeField] private Vector2 _rigidbodyVelocity;
    public Vector2 rigidbodyVelocity { get { return _rigidbodyVelocity; } }
    [Space]
    [SerializeField] private LayerMask environmentLayer;
    [Space]
    [SerializeField] private float _defaultColliderHeight = 1.8f;
    public float defaultColliderHeight { get { return _defaultColliderHeight; } }
    [SerializeField] private float _defaultColliderRadius = 0.5f;
    public float defaultColliderRadius { get { return _defaultColliderRadius; } }
    [Header("Air behaviour")]
    [Space]
    [SerializeField] private float gravity = -40;
    [SerializeField] private float minFallSpeed = -15;
    [Space]
    [SerializeField] private float fastFallGravityFactor = 3f;
    [SerializeField] private float minFastFallSpeed = -30;
    [SerializeField] private bool isFastFalling = false;
    [Space]
    [SerializeField] private float airTime;
    [Header("Ground behaviour")]
    [Space]
    [SerializeField] private bool _isGrounded = true;
    public bool isGrounded { get { return _isGrounded; } }
    [Space]
    [SerializeField] private float groundDetectionDistance = 0.2f;
    [SerializeField] private float feetDetectionOffset = 0.1f;
    [Space]
    [SerializeField][Range(0, 90)] private float _groundAngle = 0;
    public float groundAngle { get { return _groundAngle; } }
    [SerializeField][Range(0, 90)] private int _maximumGroundAngle = 45;
    public int maximumGroundAngle { get { return _maximumGroundAngle; } }
    [Header("Jump behaviour")]
    [Space]
    [SerializeField] private bool _isJumping = false;
    public bool isJumping { get { return _isJumping; } }
    [SerializeField] private bool _wallJump = false;
    public bool wallJump { get { return _wallJump; } }
    [Space]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private int jumpCount;
    [SerializeField] private bool jumpBuffer = false;
    [Space]
    [SerializeField] private float jumpStrength = 8f;
    [SerializeField] private AnimationCurve jumpDosageCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField][Range(1f, 4f)] private float shortJumpSpeedFactor = 2f;
    [Header("Wall Detection")]
    [Space]
    [SerializeField] private bool _isFacingLeftWall = false;
    public bool isFacingLeftWall { get { return _isFacingLeftWall; } }
    [SerializeField] private bool _isFacingRightWall = false;
    public bool isFacingRightWall { get { return _isFacingRightWall; } }
    [Space]
    [SerializeField] private float wallDetectionDistance = 0.2f;
    [SerializeField] private AnimationCurve wallJumpMotionFactorCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Space]
    [SerializeField][Range(0, 180)] private float wallAngle = 0;
    [SerializeField][Range(0, 180)] private int minimumWallAngle = 60;
    [SerializeField][Range(0, 180)] private int maximumWallAngle = 100;
    [Header("Edge Detection")]
    [Space]
    [SerializeField] private bool isFacingEdge = false;
    [Space]
    [SerializeField] private float minimumEdgeDetectionDistance = 0.5f;
    [SerializeField] private float maximumEdgeDetectionDistance = 1.5f;
    [Space]
    [SerializeField] private bool isFrozen = false;
    [Header("Dash Behavior")]
    [SerializeField] private bool _isDashing = false;
    public bool isDashing { get { return _isDashing; } }
    [SerializeField] private int maxDashCount;
    [SerializeField] private int DashCount;
    [SerializeField] private float DashStrength = 10f;

    [Header("COMBAT")]
    [Space(10)]
    [SerializeField] private bool hit = false;
    [Space]
    [SerializeField] private bool _isDead = false;
    public bool isDead { get { return _isDead; } }
    [SerializeField] private float resurrectDelay = 1f;

    [Header("LOCKS")]
    [Space(10)]
    [SerializeField] private bool enableGroundDetection = true;

    [Header("CHEATS")]
    [Space(10)]
    [SerializeField] private bool enableResetCharacter = false;
    [SerializeField] private bool enableInfiniteJumps = false;

    [Header("EDITOR & DEBUG")]
    [Space(10)]
    [SerializeField] private bool drawGizmos = true;

    // Singletons
    private GameManager _gameManager;
    private GameManager gameManager
    {
        get
        {
            if (!_gameManager)
                _gameManager = GameManager.Instance;

            return _gameManager;
        }
    }
    private CameraBehaviour _cameraBehaviour;
    private CameraBehaviour cameraBehaviour
    {
        get
        {
            if (!_cameraBehaviour)
                _cameraBehaviour = CameraBehaviour.Instance;

            return _cameraBehaviour;
        }
    }

    // Events
    public delegate void DefaultCallback();
    public delegate void IntCallback(int intValue);
    public delegate void FloatCallback(float floatValue);
    public delegate void RaycastHitCallback(RaycastHit hit);
    public delegate void LandingCallback(float value, RaycastHit hit);
    public delegate void CharacterControllerCallback(CharacterController characterController);
    public IntCallback OnSetPlayer;
    public IntCallback OnJump;
    public RaycastHitCallback OnWallJump;
    public LandingCallback OnLand;
    public DefaultCallback OnUTurn;
    public DefaultCallback OnResetCharacter;
    public DefaultCallback OnHit;
    public DefaultCallback OnEndHitlag;
    public DefaultCallback OnFreeze;
    public DefaultCallback OnUnfreeze;
    public CharacterControllerCallback OnDeath;
    public CharacterControllerCallback OnWaitForResurrect;
    public CharacterControllerCallback OnResurrect;

    // Cache
    public Vector3 characterCenter { get { return cachedTransform.position + capsuleCollider.center; } }
    private RaycastHit _groundHit;
    public RaycastHit groundHit { get { return _groundHit; } }
    //private RaycastHit groundEdgeHit;
    //private RaycastHit leftWallHit;
    //private RaycastHit rightWallHit;
    private RaycastHit wallHit;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private RaycastHit lastWallJumpHit;
    private bool wallHitUpperThanLastWallJump { get { return wallJump && (wallHit.normal.x * lastWallJumpHit.normal.x > 0f && wallHit.point.y > lastWallJumpHit.point.y); } }
    private float wallJumpAirTime;
    private bool isFacingAWall { get { return (isFacingLeftWall || isFacingRightWall); } }
    [Range(0, 180)] private float leftWallAngle;
    [Range(0, 180)] private float rightWallAngle;
    private Vector2 _movingVector
    {
        get
        {
            float sign = Mathf.Sign(leftStickAxisLerped.x);

            return (isGrounded && !isFacingEdge) ? (Vector2)(Quaternion.Euler(0, 0, groundAngle * Mathf.Sign(-groundHit.normal.x)) * Vector2.right * sign) : Vector2.right * sign;
        }
    }
    public Vector2 movingVector { get { return _movingVector; } }
    private float _targetSpeed { get { return isMoving ? Mathf.Lerp(minSpeed, maxSpeed, speedLerp) : 0f; } }
    public float targetSpeed { get { return _targetSpeed; } }
    private Checkpoint _resurrectCheckpoint;
    public Checkpoint resurrectCheckpoint { get { return _resurrectCheckpoint; } }
    private Vector2 _targetResurrectPos { get { return resurrectCheckpoint ? resurrectCheckpoint.GetCheckpointPosition() : startPos; } }
    public Vector2 targetResurrectPos { get { return _targetResurrectPos; } }
    private Vector2 startPos;
    private float _resurrectYOffset = 2;
    public float resurrectYOffset { get { return _resurrectYOffset; } }

    // Coroutines
    private Coroutine jumpCoroutine;
    private Coroutine jumpBufferCoroutine;
    private Coroutine uTurnCoroutine;
    private Coroutine hitCoroutine;
    private Coroutine resurrectCoroutine;

    #region UNITY_BASED

    private void Awake()
    {
#if UNITY_EDITOR
        OnJump += CheatJumpCallback;
#endif
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        FixedCacheData();

        Gravity();

        GroundDetection();
        //WallDetection();
        AirBehaviour();

        Motion();
        CharacterOrientation();
    }

    #endregion

    #region INIT_AND_MISC

    /// <summary>
    /// Returns character's Player.
    /// </summary>
    /// <param name="ID"></param>
    public Rewired.Player GetPlayer()
    {
        return player;
    }

    /// <summary>
    /// Sets playerID.
    /// </summary>
    /// <param name="ID"></param>
    public void SetPlayer(int newID)
    {
        if (newID >= 0)
        {
            _playerID = newID;
        }
        else
        {
            Debug.LogError("Player ID is incorrect. No changes made on target player.");
            return;
        }

        player = ReInput.players.GetPlayer(playerID);

        InitAllInputs(player);

        OnSetPlayer(playerID);
    }

    /// <summary>
    /// Cache values during FixedUpdate
    /// </summary>
    private void FixedCacheData()
    {
        _rigidbodyVelocity = rigid.velocity;

        GetWallHit();
    }

    #endregion

    #region INPUTS

    protected override void GetButtonUnpressed(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        base.GetButtonUnpressed(data);
    }

    protected override void GetButtonDown(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "A":
                buttonA = true;
                SetJumpBuffer();
                break;

            case "Back":
                if (!hit && !isDead)
                    Death();
                break;

            case "RB":
                DoADash();
                break;
        }

        base.GetButtonDown(data);
    }

    protected override void GetButton(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "A":
                buttonA = true;
                break;

            case "RB":
                buttonRB = true;
                break;
        }

        base.GetButton(data);
    }

    protected override void GetButtonUp(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "A":
                buttonA = false;
                //CancelJumpDosage();
                break;

        }

        base.GetButtonUp(data);
    }

    protected override void GetAxis(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "LeftStickX":
                _leftStickAxis.x = data.GetAxis();
                break;

            case "LeftStickY":
                _leftStickAxis.y = data.GetAxis();
                break;
        }

        base.GetAxis(data);
    }

    private void GetInputs()
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        if (leftStickAxis.magnitude > 1f)
        {
            _leftStickAxis = leftStickAxis.normalized;
            leftStickMagnitude = 1f;
        }
        else
        {
            leftStickMagnitude = leftStickAxis.magnitude;
        }

        if (leftStickDeadZone == 0f)
        {
            _leftStickAxisLerped = leftStickAxis;
            leftStickMagnitudeLerped = leftStickMagnitude;
        }
        else
        {
            float xLerp = MathUtils.SignedLerp(leftStickAxis.x, leftStickDeadZone, 1f);
            float yLerp = MathUtils.SignedLerp(leftStickAxis.y, leftStickDeadZone, 1f);
            _leftStickAxisLerped = new Vector2(xLerp, yLerp);
            leftStickMagnitudeLerped = leftStickAxisLerped.magnitude;
        }
    }

    #endregion

    #region BEHAVIOUR

    private void Gravity()
    {
        // Character physics is frozen
        if (isFrozen)
            return;

        if (isGrounded && !isFacingEdge)
            return;

        if (isJumping)
            return;

        if (isDashing)
            return;

        if (!isFastFalling && !isGrounded && !isJumping && rigid.velocity.y < 0f && leftStickAxisLerped.y < -0.5f)
            isFastFalling = true;

        float targetGravity = isFastFalling ? gravity * fastFallGravityFactor : gravity;
        rigid.AddForce(Vector3.up * targetGravity, ForceMode.Acceleration);

        float targetFallSpeed = isFastFalling ? minFastFallSpeed : minFallSpeed;
        if (rigid.velocity.y < targetFallSpeed)
            SetRigidbodyVelocity(new Vector3(rigid.velocity.x, targetFallSpeed, 0));
    }

    private void Motion()
    {
        _isMoving = Mathf.Abs(leftStickAxisLerped.x) != 0f;

        if (isMoving)
        {
            if (isGrounded && !uTurn)
                _leftRight = (int)Mathf.Sign(leftStickAxisLerped.x);
        }

        // Character physics is frozen
        if (isFrozen)
            return;

        // Stop motion if player moves towards a wall
        //if ((wallAngle > maximumGroundAngle) && (leftStickAxisLerped.x * wallHit.normal.x < 0f))
        //{
        //    _isMoving = false;

        //    if (isGrounded)
        //        SetRigidbodyVelocity(Vector3.zero);

        //    return;
        //}

        // Is doing a U Turn
        if (uTurn)
            return;

        _speedLerp = Mathf.InverseLerp(0f, runSpeedThresold, Mathf.Abs(leftStickAxisLerped.x));

        // Ground tilt speed
        Vector2 targetVector = _movingVector * targetSpeed;
        float xVelocityLerp = Mathf.Lerp(rigidbodyVelocity.x, targetVector.x, speedLerpRate * gameManager.fixedDeltaTime);
        Vector2 velocityLerp = new Vector2(xVelocityLerp, targetVector.y);

        if (!isGrounded) // Air movements
        {
            if (wallJump)
                velocityLerp = Vector2.Lerp(rigidbodyVelocity, velocityLerp, wallJumpMotionFactorCurve.Evaluate(airTime - wallJumpAirTime));

            velocityLerp.y = rigid.velocity.y;
        }

        currentSpeed = velocityLerp.magnitude;
        SetRigidbodyVelocity(velocityLerp);

        // U Turn
        if (!uTurn && isGrounded && isMoving && // Default conditions
            (leftRight * rigid.velocity.x < 0f) // Stick on opposite side
                                                //&& Mathf.Abs(rigidbodyVelocity.x) > 2f) // Character was moving fast
            && currentSpeed > 1f)
        {
            UTurn();
        }
    }

    private void CharacterOrientation(bool forceOrientation = false)
    {
        // Character physics is frozen
        if (isFrozen)
            return;

        if (!forceOrientation)
        {
            if (uTurn)
                return;
        }

        characterBehaviour.transform.rotation = Quaternion.Slerp(characterBehaviour.transform.rotation, Quaternion.Euler(0, _leftRight * 90.1f, 0), forceOrientation ? 1 : 10f * gameManager.fixedDeltaTime);
    }

    private void GroundDetection()
    {
        // Ground detection explicitely disabled
        if (!enableGroundDetection)
            return;

        // Do not detect ground while jumping
        if (isJumping)
            return;

        // When grounded, the cast is a sphere, else it's a ray
        bool raycast = isGrounded ?
            Physics.SphereCast(
                cachedTransform.position + new Vector3(0, capsuleCollider.radius + feetDetectionOffset, 0),
                capsuleCollider.radius, Vector3.down, out _groundHit,
                groundDetectionDistance, environmentLayer) :
            Physics.Raycast(
                characterCenter,
                Vector3.down, out _groundHit,
                capsuleCollider.radius + groundDetectionDistance, environmentLayer);

        if (raycast) // Ground detected
        {
            GroundAttach();
        }
        else // No ground detected
        {
            GroundDetach();
        }

        // Edge detection
        isFacingEdge = EdgeCast();
    }

    private void GroundAttach(bool forceGroundDetection = false)
    {
        // Catch landing position
        if (forceGroundDetection)
        {
            Physics.SphereCast(
                cachedTransform.position + new Vector3(0, capsuleCollider.radius + feetDetectionOffset, 0),
                capsuleCollider.radius, Vector3.down, out _groundHit,
                groundDetectionDistance, environmentLayer);
        }

        _groundAngle = GetFaceAngleFromNormal(Vector3.up, groundHit.normal);
        if (groundAngle > maximumGroundAngle)
        {
            GroundDetach();
            return;
        }

        // Landing
        if (!isGrounded)
        {
            // Character is moving backwards
            if (leftRight * rigidbodyVelocity.x < 0)
            {
                if (isMoving)
                    _leftRight *= -1;

                if (rigidbodyVelocity.y > -2f)
                    CharacterOrientation(true);
            }

            isFastFalling = false;
            _wallJump = false;

            SetColliderMode(0);

            OnLand(rigid.velocity.y, groundHit);

            SetRigidbodyVelocity(new Vector3(rigidbodyVelocity.x, 0, 0));
        }

        if (!isFacingEdge)
            SetRigidbodyPosition(GetSphereHitPosition(groundHit));

        _isGrounded = true;
        jumpCount = 0;

        if (jumpBuffer)
            Jump(0);
    }

    private void GroundDetach()
    {
        // Ground just not found (air start)
        if (isGrounded)
        {
            SetColliderMode(1);
        }

        if (uTurn)
            CancelUTurn();

        _groundAngle = 0;

        // Cancel first jump
        if (jumpCount == 0)
            jumpCount++;

        _isGrounded = false;
    }

    private bool EdgeCast()
    {
        if (!isGrounded)
            return false;

        return !Physics.Raycast(
                characterCenter + new Vector3((isMoving ? Mathf.Sign(leftStickAxisLerped.x) : leftRight) * capsuleCollider.radius, 0, 0),
                Vector3.down, capsuleCollider.height / 2f + Mathf.Lerp(minimumEdgeDetectionDistance, maximumEdgeDetectionDistance, groundAngle / maximumGroundAngle), environmentLayer);
    }

    private Vector3 GetSphereHitPosition(RaycastHit hit)
    {
        return hit.point + (hit.normal - Vector3.up) * capsuleCollider.radius;
    }

    private Vector3 GetRayHitPosition(RaycastHit hit)
    {
        return hit.point + (hit.normal - Vector3.up) * capsuleCollider.radius;
    }

    private void WallDetection()
    {
        _isFacingLeftWall = SideWallDetection(-1);
        _isFacingRightWall = SideWallDetection(1);

        if (jumpBuffer && CheckWallJump() && isFacingAWall)
            Jump(1);
    }

    private bool SideWallDetection(int direction)
    {
        float xDetectionOffset = 0.1f;
        Vector3 defaultPos = cachedTransform.position + new Vector3(-direction * xDetectionOffset, capsuleCollider.center.y, 0);
        RaycastHit hit;

        // When grounded, the cast is a capsule, else it's a sphere
        bool hitState = false;

        if (isGrounded)
        {
            float capsuleYOffsetFromCenter = (capsuleCollider.height - (capsuleCollider.radius * 2f)) / 2f;

            hitState = Physics.CapsuleCast(
                defaultPos + new Vector3(0, -capsuleYOffsetFromCenter + feetDetectionOffset, 0), // Bottom sphere
                defaultPos + new Vector3(0, capsuleYOffsetFromCenter, 0), // Top sphere
                capsuleCollider.radius,
                new Vector3(direction, 0, 0),
                out hit,
                wallDetectionDistance,
                environmentLayer);
        }
        else
        {
            hitState = Physics.SphereCast(
                characterCenter + new Vector3(-direction * xDetectionOffset, 0, 0),
                capsuleCollider.radius,
                new Vector3(direction, 0, 0),
                out hit,
                wallDetectionDistance,
                environmentLayer);
        }

        if (hitState)
        {
            float angle = GetFaceAngleFromNormal(Vector3.up, hit.normal);
            if (direction == -1)
            {
                leftWallHit = hit;
                leftWallAngle = angle;
            }
            if (direction == 1)
            {
                rightWallHit = hit;
                rightWallAngle = angle;
            }

            wallAngle = angle;

            // The raycasted "wall" is actually a walkable surface
            if (!isJumping && wallAngle < maximumGroundAngle)
            {
                GroundAttach(true);
                return false;
            }

            if (angle < minimumWallAngle || angle > maximumWallAngle)
                return false;
        }

        return hitState;
    }

    private void GetWallHit()
    {
        if (isFacingLeftWall)
        {
            wallHit = leftWallHit;
            wallAngle = leftWallAngle;
        }
        else if (isFacingRightWall)
        {
            wallHit = rightWallHit;
            wallAngle = rightWallAngle;
        }
        else
        {
            wallAngle = 0;
        }
    }

    private void AirBehaviour()
    {
        if (isGrounded)
        {
            airTime = 0f;
            return;
        }

        airTime += gameManager.fixedDeltaTime;
    }

    private void SetRigidbodyVelocity(Vector2 vel, string message = "")
    {
        rigid.velocity = vel;
        _rigidbodyVelocity = vel;

        if (!string.IsNullOrEmpty(message))
            Debug.Log(message);
    }

    public void SetRigidbodyPosition(Vector2 pos, bool forcePos = false)
    {
        if (forcePos)
            cachedTransform.position = pos;

        rigid.MovePosition(pos);
    }

    private float GetFaceAngleFromNormal(Vector3 direction, Vector3 normal)
    {
        normal.z = 0;
        float dot = Vector3.Dot(direction, normal.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        return angle;
    }

    private void SetJumpBuffer()
    {
        bool canWallJump = CheckWallJump();
        bool canJump = jumpCount < maxJumpCount;

        if (canWallJump)
        {
            Jump(1);
        }
        else if (canJump)
        {
            Jump(0);
        }
        else // Set a short buffer if the character cannot jump
        {
            if (jumpBufferCoroutine != null)
                StopCoroutine(jumpBufferCoroutine);
            jumpBufferCoroutine = StartCoroutine(CoSetJumpBuffer());
        }
    }

    private IEnumerator CoSetJumpBuffer()
    {
        jumpBuffer = true;
        yield return new WaitForSecondsRealtime(0.2f);
        jumpBuffer = false;
    }

    private void Jump(int jumpMode)
    {
        // Character physics is frozen
        if (isFrozen)
            return;

        if (hit)
            return;

        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);
        jumpCoroutine = StartCoroutine(JumpDosage());

        CancelUTurn();

        _isJumping = true;
        jumpBuffer = false;
        _wallJump = false;
        isFastFalling = false;
        isFacingEdge = false;

        SetColliderMode(1);

        if (jumpMode == 0)
            DoAJump();
        else
            DoAWallJump();
    }

    private void DoAJump()
    {
        OnJump(jumpCount);

        _isGrounded = false;
        _groundAngle = 0;

        SetRigidbodyVelocity(new Vector3(rigid.velocity.x, jumpStrength, 0f));

        jumpCount++;
    }

    private void DoAWallJump()
    {
        GetWallHit();

        int direction = isFacingLeftWall ? 1 : -1;

        SetRigidbodyVelocity(new Vector3(maxSpeed * direction, jumpStrength, 0f));
        SetRigidbodyPosition(GetSphereHitPosition(wallHit));
        _leftRight = direction;
        CharacterOrientation(true);
        wallJumpAirTime = airTime;
        _wallJump = true;

        lastWallJumpHit = wallHit;

        OnWallJump(wallHit);
    }

    private bool CheckWallJump()
    {
        if (isGrounded)
            return false;

        // Cannot make a wall jump on the same direction, upper than the previous one
        if (wallHitUpperThanLastWallJump)
            return false;

        return isFacingAWall;
    }

    private IEnumerator JumpDosage()
    {
        float t = 0;
        Keyframe lastKey = jumpDosageCurve.keys[jumpDosageCurve.keys.Length - 1];

        while (t < lastKey.time)
        {
            float holdFactor = buttonA ? 1f : shortJumpSpeedFactor;
            t += gameManager.deltaTime * holdFactor;
            float jumpDosageStrength = jumpDosageCurve.Evaluate(t);

            //rigid.AddForce(Vector3.up * jumpDosageStrength, ForceMode.Acceleration);
            SetRigidbodyVelocity(new Vector3(rigid.velocity.x, jumpDosageStrength, 0));

            yield return null;
        }

        _isJumping = false;
    }

    private void CancelJumpDosage()
    {
        if (!isJumping)
            return;

        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);

        _isJumping = false;
    }

    private void UTurn()
    {
        if (uTurnCoroutine != null)
            StopCoroutine(uTurnCoroutine);
        uTurnCoroutine = StartCoroutine(CoUTurn());

        OnUTurn();
    }

    private IEnumerator CoUTurn()
    {
        float t = 0f;
        float startXVelocity = rigidbodyVelocity.x;

        _uTurn = true;

        _leftRight = (int)Mathf.Sign(leftStickAxisLerped.x);
        CharacterOrientation(true);

        while (t < uTurnDelay)
        {
            t += gameManager.deltaTime;
            float lerp = 1f - (t / uTurnDelay);

            SetRigidbodyVelocity(new Vector3(lerp * startXVelocity, rigidbodyVelocity.y, 0));

            yield return null;
        }

        _uTurn = false;
    }

    private void CancelUTurn()
    {
        if (!uTurn)
            return;

        if (uTurnCoroutine != null)
            StopCoroutine(uTurnCoroutine);

        _uTurn = false;
    }

    private void SetColliderMode(int mode)
    {
        SetColliderParameters(capsuleCollider.radius, mode == 0 ? _defaultColliderHeight : _defaultColliderRadius * 2f, defaultColliderHeight / 2f);
    }

    public void SetColliderParameters(float radius, float height, float yPos)
    {
#if UNITY_EDITOR
        if (!_capsuleCollider)
            _capsuleCollider = capsuleCollider;
#endif

        _capsuleCollider.radius = radius;
        _capsuleCollider.height = height;
        _capsuleCollider.center = new Vector3(0, yPos, 0);
    }

    private void FreezeCharacter()
    {
        isFrozen = true;

        CancelJumpDosage();
        CancelUTurn();

        buttonA = false;
        _isMoving = false;
        isFastFalling = false;
        _wallJump = false;

        SetRigidbodyVelocity(Vector3.zero);

        OnFreeze();
    }

    private void UnfreezeCharacter()
    {
        isFrozen = false;

        OnUnfreeze();
    }

    public void SetStartPosition(Vector2 newStartPos)
    {
        startPos = newStartPos;
    }

    public void DoADash()
    {
        if (isFrozen)
            return;

        if (hit)
            return;

        Debug.Log("Do a Dash");

        Debug.Log(leftRight);

        //SetRigidbodyVelocity(new Vector3(_rigid.velocity.x * 2 * leftRight * DashStrength, 0f, 0f));

    }

    #endregion

    #region COMBAT

    public void Hit(float hitLagDuration = 0.2f)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(CoHit(hitLagDuration));

        OnHit();
    }

    private IEnumerator CoHit(float hitLagDuration)
    {
        hit = true;
        FreezeCharacter();

        float t = 0f;
        while (t < hitLagDuration)
        {
            t += gameManager.deltaTime;

            yield return null;
        }

        hit = false;
        UnfreezeCharacter();

        OnEndHitlag();

        Death();
    }

    private void CancelHit()
    {
        if (!hit)
            return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hit = false;

        OnEndHitlag();
    }

    public void Death()
    {
        FreezeCharacter();

        _isDead = true;

        capsuleCollider.enabled = false;

        resurrectCoroutine = StartCoroutine(WaitForResurrect());

        OnDeath(this);
    }

    private IEnumerator WaitForResurrect()
    {
        yield return new WaitForSeconds(resurrectDelay);

        OnWaitForResurrect(this);

        if (!gameManager.isSoloMode)
        {
            // Wait until resurrect position is within camera bounds
            yield return new WaitUntil(() => cameraBehaviour.IsResurrectPositionInsideCameraBounds(this));
        }

        Resurrect();
    }

    public void CancelWaitForResurrect()
    {
        if (!isDead)
            return;

        if (resurrectCoroutine != null)
            StopCoroutine(resurrectCoroutine);

        Resurrect();
    }

    public void SetResurrectCheckpoint(Checkpoint checkpoint)
    {
        _resurrectCheckpoint = checkpoint;
    }

    public void Resurrect()
    {
        ResetCharacter(targetResurrectPos + new Vector2(0, resurrectYOffset));

        _isDead = false;
        capsuleCollider.enabled = true;

        OnResurrect(this);
    }

    public void Resurrect(Vector2 forcePos)
    {
        ResetCharacter(forcePos + new Vector2(0, resurrectYOffset));

        _isDead = false;

        OnResurrect(this);
    }

    private void ResetCharacter(Vector3 pos)
    {
        if (!enableResetCharacter)
            return;

        UnfreezeCharacter();

        CancelJumpDosage();
        CancelUTurn();
        //CancelWaitForResurrect();
        CancelHit();

        SetColliderMode(0);

        _isGrounded = true;
        buttonA = false;
        _isMoving = false;
        isFastFalling = false;
        _wallJump = false;

        jumpCount = 0;
        airTime = 0;

        SetRigidbodyVelocity(Vector3.zero);
        transform.position = pos;
        SetRigidbodyPosition(pos);

        OnResetCharacter();
    }

    #endregion

    #region DEBUG_AND_EDITOR

    public void UpdateColliderParameters()
    {
        _defaultColliderRadius = capsuleCollider.radius;
        _defaultColliderHeight = capsuleCollider.height;
    }

    public void SetAllCheats(bool enabled)
    {
        enableResetCharacter = enabled;
        enableInfiniteJumps = enabled;
    }

    private void CheatJumpCallback(int count)
    {
        if (!enableInfiniteJumps)
            return;

        if (count == maxJumpCount - 1)
            jumpCount--;
    }

    #endregion
}
