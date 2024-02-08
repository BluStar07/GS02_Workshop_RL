using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Rewired;
using System.Collections.Generic;
using System.Collections;

public partial class CameraBehaviour : InputListener {

    private static CameraBehaviour _Instance;
    public static CameraBehaviour Instance { get { return _Instance; } }

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

	[Header("COMPONENTS")]
	[Space(10)]
	[SerializeField] private Camera _cam;
	public Camera cam { get { return _cam; } }
	[SerializeField] private List<CharacterController> _characters = new List<CharacterController>();
	public List<CharacterController> characters { get { return _characters; } }
    [Space]
    [SerializeField] private BoxCollider cameraColliderLeft;
    [SerializeField] private BoxCollider cameraColliderRight;
    [SerializeField] private BoxCollider cameraColliderUp;
    [Space]
    [SerializeField] private BoxCollider cameraDeathTriggerLeft;
    [SerializeField] private BoxCollider cameraDeathTriggerRight;
    [SerializeField] private BoxCollider cameraDeathTriggerUp;
    [SerializeField] private BoxCollider cameraDeathTriggerDown;

    [Header("INPUTS")]
    [Space(10)]
    [SerializeField] private Vector2 _rightStickAxis;
    public Vector2 rightStickAxis { get { return _rightStickAxis; } }
    [SerializeField] [Range(0f, 1f)] private float rightStickMagnitude;
    [Space]
    [SerializeField] [Range(0f, 1f)] private float _rightStickDeadZone;
    public float rightStickDeadZone { get { return _rightStickDeadZone; } }
    [Space]
    [SerializeField] private Vector2 _rightStickAxisLerped;
    public Vector2 rightStickAxisLerped { get { return _rightStickAxisLerped; } }
    [SerializeField] [Range(0f, 1f)] private float rightStickMagnitudeLerped;

    [Header("BEHAVIOUR")]
	[Space(10)]
	[SerializeField] private Vector2 positionOffset;
	[SerializeField] private float positionLerpRate = 8f;
	private Vector2 targetPosition;
    private Vector2 targetPositionUnclamped;
    [Space]
    [SerializeField] private Vector2 followVector;
    [SerializeField] private Vector2 maxFollowVelocity = new Vector2(16, 9) * 0.25f;
    [SerializeField] private float followFactor = 0.5f;
    [SerializeField] private float followLerpRate = 2f;
    [Space]
    [SerializeField] private Vector2 inputOffset;
    [SerializeField]
    private Vector2 inputOffsetStrength
    {
        get
        {
            Vector2 maxOffset = (maxFrame - charactersSpacing) / 2f - charactersEdgeSpacing;
            if (maxOffset.x < 0f)
                maxOffset.x = 0f;
            if (maxOffset.y < 0f)
                maxOffset.y = 0f;

            return maxOffset;
        }
    }
    [SerializeField] private float inputOffsetLerpRate = 4f;
    [Space]
    [SerializeField] private Vector2 _minFrame = new Vector2(16, 9) / 2;
	public Vector2 minFrame { get { return _minFrame; } }
	[SerializeField] private Vector2 _maxFrame = new Vector2(16, 9);
	public Vector2 maxFrame { get { return _maxFrame; } }
    [Space]
    [SerializeField] private Vector2 _minCharactersFrame = new Vector2(24, 16) / 2;
    public Vector2 minCharactersFrame { get { return _minCharactersFrame; } }
    [SerializeField] private Vector2 _charactersEdgeSpacing = new Vector2(4, 4);
    public Vector2 charactersEdgeSpacing { get { return _charactersEdgeSpacing; } }
    public Vector2 maxCharactersFrame { get { return maxFrame - charactersEdgeSpacing * 2; } }
    [Space]
    [SerializeField] [Range(0f, 1f)] private float currentZoom = 1f;
    [Space]
    [SerializeField] [Range(0f, 1f)] private float targetSpacingZoom = 1f;
    [SerializeField] [Range(0f, 1f)] private float targetInputZoom = 1f;
    [Space]
    [SerializeField] [Range(0f, 1f)] private float targetZoom = 1f;
	[SerializeField] [Range(0f, 1f)] private float maximumZoom = 1f;
	[SerializeField] private float zoomLerpRate = 8f;
    [Space]
    [SerializeField] private Vector2 _minWorldLimits = new Vector2(-20, -15);
    public Vector2 minWorldLimits { get { return _minWorldLimits; } }
    [SerializeField] private Vector2 _maxWorldLimits = new Vector2(20, 15);
    public Vector2 maxWorldLimits { get { return _maxWorldLimits; } }
    [Space]
    [SerializeField] private float cameraCollidersThickness = 1f;
    [SerializeField] private float cameraDeathTriggersThickness = 2f;
    [SerializeField] private float downCameraDeathTriggersYOffset = 2f;

    [Header("EFFECTS")]
    [Space(10)]
    [SerializeField] private BlurOptimized blur;

    [Header("EDITOR & DEBUG")]
    [Space(10)]
    [SerializeField] private bool drawGizmos = true;

    [Space(10)]
    [SerializeField] private float yOffset;


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
    private UI_MainCanvas _mainCanvas;
    private UI_MainCanvas mainCanvas
    {
        get
        {
            if (!_mainCanvas)
                _mainCanvas = UI_MainCanvas.Instance;

            return _mainCanvas;
        }
    }

    // Cache
    public Vector2 currentFrame { get { return GetFrameFromZoomLerp(currentZoom); } }
    public Vector2 targetFrame { get { return GetFrameFromZoomLerp(targetZoom); } }
    public float cameraAspect { get { return cam.aspect; } }
    private Rewired.Player player;
    private Vector2 charactersSpacing { get { return maxCharactersPos - minCharactersPos; } }
    private Vector2 charactersSpacingFromCurrentEdges
    {
        get
        {
            float minXOffset = Mathf.Abs(minCharactersPos.x - minCameraCorner.x);
            float maxXOffset = Mathf.Abs(maxCharactersPos.x - maxCameraCorner.x);
            float xOffset = minXOffset < maxXOffset ? minXOffset : maxXOffset;

            float minYOffset = Mathf.Abs(minCharactersPos.y - minCameraCorner.y);
            float maxYOffset = Mathf.Abs(maxCharactersPos.y - maxCameraCorner.y);
            float yOffset = minYOffset < maxYOffset ? minYOffset : maxYOffset;

            return new Vector2(xOffset, yOffset);
        }
    }
    private Vector2 minCharactersPos;
    private Vector2 maxCharactersPos;
    private Vector2 charactersCenter { get { return minCharactersPos + (charactersSpacing / 2f); } }

    private Vector2 _minCameraCorner { get { return (Vector2)cachedTransform.position - currentFrame / 2f; } }
    public Vector2 minCameraCorner { get { return _minCameraCorner; } }
    private Vector2 _maxCameraCorner { get { return (Vector2)cachedTransform.position + currentFrame / 2f; } }
    public Vector2 maxCameraCorner { get { return _maxCameraCorner; } }

    public bool isEveryoneOnScreen
    {
        get
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (!IsCharacterOnScreen(characters[i]))
                    return false;
            }

            return true;
        }
    }

    private Vector2 inputStrengthFactor;

    // Coroutines
    private Coroutine disableCollidersCoroutine;

    #region UNITY_BASED

    private void Awake()
	{
        if (!_Instance)
        {
            _Instance = this;
        }
        else
        {
            Debug.LogError("Two instances of CameraBehaviour detected. Removing the latest one.");
            Destroy(gameObject);
            return;
        }
    }

	private void Start()
	{
		Init();
        InitEvents();
	}

	private void Update ()
	{
        GetInputs();

        GetCharactersMinMaxPosition();
        SetCameraGroupPosition();
		SetTargetZoomLerp();
		Zoom();

        UpdateCameraColliders();
    }

    #endregion

    #region INIT_AND_MISC

    private void Init ()
	{
		if (!cam)
			GetCamera();

        SetBlur(false);

        GetPlayer();

        inputOffset = Vector2.zero;
        currentZoom = targetZoom = 1;
    }

    private void InitEvents()
    {
        gameManager.OnPlayerAdded += SetCameraColliders;
        gameManager.OnPlayerRemoved += SetCameraColliders;
    }

    public void GetPlayer()
    {
        player = ReInput.players.GetPlayer(0);

        if (player != null)
            Debug.Log("Player " + player.name + " assigned for camera controls.");
    }

    public void GetCamera ()
	{
		_cam = GetComponentInChildren<Camera>();
	}

	public void GetCharacters ()
	{
		_characters = new List<CharacterController>(FindObjectsOfType<CharacterController>());

        // Events
        if (characters.Count == 0)
            return;

        characters[0].OnResetCharacter += ResetOffsets;
    }

    public void SetCharacters(List<CharacterController> characterControllersList, bool gameStart = false)
    {
        if (!gameStart)
        {
            // Unsubscribe from "first" player inputs
            if (characters.Count > 0)
                //UnsubscribeAllInputs(characters[0].GetPlayer()); 

            // Unsubscribe from all characters  events
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].OnResurrect -= PlayerResurrect;
            }
        }

        // Assign new characters array
        _characters = new List<CharacterController>(characterControllersList);

        // Subscribe to characters events
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].OnResurrect += PlayerResurrect;
        }

        // Input offset can only be triggered by the first player alive on the characters list
        if (characters.Count > 0)
        {
            //InitAllInputs(characters[0].GetPlayer());
            mainCanvas.playersPanel.SetPlayerCamera(characters[0].playerID);
        }

        // Edit camera colliders depending on characters alive count
        SetCameraColliders();
    }

    #endregion

    #region INPUTS

    protected override void GetAxis(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "RightStickX":
                _rightStickAxis.x = data.GetAxis(); ;
                break;

            case "RightStickY":
                _rightStickAxis.y = data.GetAxis();
                break;
        }

        base.GetAxis(data);
    }

    private void GetInputs()
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        if (characters[0].isDead)
        {
            _rightStickAxis = _rightStickAxisLerped = Vector2.zero;
            rightStickMagnitude = rightStickMagnitudeLerped = 0f;
            return;
        }

        if (rightStickAxis.magnitude > 1f)
        {
            _rightStickAxis = rightStickAxis.normalized;
            rightStickMagnitude = 1f;
        }
        else
        {
            rightStickMagnitude = rightStickAxis.magnitude;
        }

        if (rightStickDeadZone == 0f)
        {
            _rightStickAxisLerped = rightStickAxis;
            rightStickMagnitudeLerped = rightStickMagnitude;
        }
        else
        {
            float xLerp = MathUtils.SignedLerp(rightStickAxis.x, rightStickDeadZone, 1f);
            float yLerp = MathUtils.SignedLerp(rightStickAxis.y, rightStickDeadZone, 1f);
            _rightStickAxisLerped = new Vector2(xLerp, yLerp);
            rightStickMagnitudeLerped = rightStickAxisLerped.magnitude;
        }
    }

    #endregion

    #region BEHAVIOUR

    public void SetCameraGroupPosition (bool instantPos = false)
	{
        if (characters.Count == 0)
        {
            cachedTransform.position = Vector2.Lerp(cachedTransform.position, targetPosition, instantPos ? 1f : positionLerpRate * gameManager.deltaTime);
            return;
        }

        // Main position
        targetPosition = charactersCenter;

        // Follow vector (Solo mode only)
        if (characters.Count == 1)
        {
            if (characters[0].isMoving)
            {
                Vector2 followOffset = characters[0].rigidbodyVelocity * followFactor;
                followOffset = new Vector2(
                    Mathf.Clamp(followOffset.x, -maxFollowVelocity.x, maxFollowVelocity.x),
                    Mathf.Clamp(followOffset.y, -maxFollowVelocity.y, maxFollowVelocity.y));

                followVector = Vector2.Lerp(followVector, followOffset, instantPos ? 1f : followLerpRate * gameManager.deltaTime);
            }
            else if (rightStickAxisLerped.magnitude > rightStickDeadZone) // Input detected
            {
                followVector = Vector2.Lerp(followVector, Vector2.zero, instantPos ? 1f : followLerpRate * 2f * gameManager.deltaTime);
            }
        }
        else
        {
            followVector = Vector2.Lerp(followVector, Vector2.zero, instantPos ? 1f : followLerpRate * 2f * gameManager.deltaTime);
        }

        targetPosition += followVector;

        // Input offset
        Vector2 targetInputOffset = rightStickAxisLerped * inputOffsetStrength;
        // Clamp input offset to world limits
        targetInputOffset.x = Mathf.Clamp(targetInputOffset.x, minWorldLimits.x - minCameraCorner.x, maxWorldLimits.x - maxCameraCorner.x);
        targetInputOffset.y = Mathf.Clamp(targetInputOffset.y, minWorldLimits.y - minCameraCorner.y, maxWorldLimits.y - maxCameraCorner.y);
        // Lerp input offset
        inputOffset = Vector2.Lerp(inputOffset, targetInputOffset, instantPos ? 1f : inputOffsetLerpRate * gameManager.deltaTime);

        targetPosition += inputOffset + new Vector2(0,yOffset);

        // Final output
        Vector2 minPos = minWorldLimits + currentFrame / 2f;
        Vector2 maxPos = maxWorldLimits - currentFrame / 2f;

        targetPositionUnclamped = targetPosition;
        targetPosition = new Vector2(
            Mathf.Clamp(targetPosition.x, minPos.x, maxPos.x),
            Mathf.Clamp(targetPosition.y, minPos.y, maxPos.y));

        cachedTransform.position = Vector2.Lerp(cachedTransform.position, targetPosition, instantPos ? 1f : positionLerpRate * gameManager.deltaTime);
	}

	public void SetTargetZoomLerp ()
	{
        // Lerp from characters spacing
        float xLerp = Mathf.InverseLerp(minCharactersFrame.x, maxCharactersFrame.x, charactersSpacing.x);
        float yLerp = Mathf.InverseLerp(minCharactersFrame.y, maxCharactersFrame.y, charactersSpacing.y);
        targetSpacingZoom = Mathf.Clamp(xLerp > yLerp ? 1 - xLerp : 1 - yLerp, 0f, maximumZoom);

        float inputXLerp = inputOffsetStrength.x == 0 ? 0 : Mathf.Abs(inputOffset.x) / inputOffsetStrength.x;
        float inputYLerp = inputOffsetStrength.y == 0 ? 0 : Mathf.Abs(inputOffset.y) / inputOffsetStrength.y;
        targetInputZoom = Mathf.Clamp(inputXLerp > inputYLerp ? 1 - inputXLerp : 1 - inputYLerp, 0f, maximumZoom);

        // Final output takes the higher lerp value
        targetZoom = targetSpacingZoom < targetInputZoom ? targetSpacingZoom : targetInputZoom;
    }

    public void GetCharactersMinMaxPosition()
    {
        if (characters.Count == 0)
            return;

        minCharactersPos = maxCharactersPos = characters[0].characterCenter;
        for (int i = 1; i < characters.Count; i++)
        {
            Vector2 characterCenter = characters[i].characterCenter;

            if (characterCenter.x < minCharactersPos.x)
                minCharactersPos.x = characterCenter.x;
            if (characterCenter.x > maxCharactersPos.x)
                maxCharactersPos.x = characterCenter.x;

            if (characterCenter.y < minCharactersPos.y)
                minCharactersPos.y = characterCenter.y;
            if (characterCenter.y > maxCharactersPos.y)
                maxCharactersPos.y = characterCenter.y;
        }
    }

    public void Zoom(bool instantZoom = false)
	{
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, instantZoom ? 1 : zoomLerpRate * gameManager.deltaTime);

        float camDistance = currentFrame.y * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        cam.transform.localPosition = new Vector3(0, 0, -camDistance);
	}

    private Vector2 GetFrameFromZoomLerp(float zoomLerp)
    {
        zoomLerp = Mathf.Clamp(zoomLerp, 0f, 1f);
        return Vector2.Lerp(minFrame, maxFrame, 1 - zoomLerp);
    }

	public void SetMinFrame (Vector2 frame)
	{
		_minFrame = frame;
	}

	public void SetMaxFrame(Vector2 frame)
	{
		_maxFrame = frame;
	}

    public void SetBlur (bool set)
    {
        if (blur.enabled = set)
            return;

        blur.enabled = set;
    }

    private void ResetOffsets()
    {
        followVector = inputOffset = Vector2.zero;
    }

    private void PlayerResurrect(CharacterController characterController)
    {
        //if (characters.Count == 1)
            ResetOffsets();
    }

    public void UpdateCameraColliders ()
    {
        int zSize = 4;

        // Camera colliders
        Vector3 sideCollidersSize = new Vector3(cameraCollidersThickness, currentFrame.y + downCameraDeathTriggersYOffset, zSize);
        Vector3 upColliderSize = new Vector3(currentFrame.x + cameraCollidersThickness * 2, cameraCollidersThickness, zSize);

        cameraColliderLeft.size = cameraColliderRight.size = sideCollidersSize;
        cameraColliderUp.size = upColliderSize;

        Vector3 sideCollidersCenter = new Vector3((currentFrame.x + cameraColliderRight.size.x) / 2f, -downCameraDeathTriggersYOffset / 2f, 0);

        cameraColliderLeft.center = new Vector3(-sideCollidersCenter.x, sideCollidersCenter.y, 0);
        cameraColliderRight.center = sideCollidersCenter;
        cameraColliderUp.center = new Vector3(0, (currentFrame.y + cameraColliderUp.size.y) / 2f, 0);

        cameraColliderLeft.transform.localPosition = cameraColliderLeft.transform.localEulerAngles = Vector3.zero;
        cameraColliderLeft.transform.localScale = Vector3.one;

        cameraColliderLeft.isTrigger = cameraColliderRight.isTrigger = cameraColliderUp.isTrigger = false;

        // Camera death triggers
        Vector3 sideDeathTriggersSize = new Vector3(cameraDeathTriggersThickness, currentFrame.y + cameraCollidersThickness + downCameraDeathTriggersYOffset, zSize);
        Vector3 upDownDeathTriggersSize = new Vector3(upColliderSize.x + cameraDeathTriggersThickness * 2, cameraDeathTriggersThickness, zSize);

        float centerOffset = (cameraCollidersThickness + cameraDeathTriggersThickness) / 2f;

        cameraDeathTriggerLeft.size = cameraDeathTriggerRight.size = sideDeathTriggersSize;
        cameraDeathTriggerUp.size = cameraDeathTriggerDown.size = upDownDeathTriggersSize;

        Vector3 sideDeathCollidersCenter = cameraColliderRight.center + new Vector3(centerOffset, cameraCollidersThickness / 2f, 0);

        cameraDeathTriggerLeft.center = new Vector3(-sideDeathCollidersCenter.x, sideDeathCollidersCenter.y, 0);
        cameraDeathTriggerRight.center = sideDeathCollidersCenter;

        cameraDeathTriggerUp.center = cameraColliderUp.center + new Vector3(0, centerOffset, 0);
        cameraDeathTriggerDown.center = new Vector3(0, -(currentFrame.y + cameraDeathTriggersThickness) / 2f - downCameraDeathTriggersYOffset, 0);

        cameraDeathTriggerLeft.transform.localPosition = cameraDeathTriggerLeft.transform.localEulerAngles = Vector3.zero;
        cameraDeathTriggerLeft.transform.localScale = Vector3.one;

        cameraDeathTriggerLeft.isTrigger = cameraDeathTriggerRight.isTrigger = cameraDeathTriggerUp.isTrigger = cameraDeathTriggerDown.isTrigger = true;
    }

    private void SetCameraColliders ()
    {
        bool isEnabled = !gameManager.isSoloMode;

        cameraColliderLeft.gameObject.SetActive(isEnabled);
    }

    private void SetCameraColliders(bool isEnabled)
    {
        cameraColliderLeft.gameObject.SetActive(isEnabled);
    }

    private void SetCameraDeathTriggers(bool isEnabled)
    {
        cameraDeathTriggerLeft.gameObject.SetActive(isEnabled);
    }

    public bool IsResurrectPositionInsideCameraBounds(CharacterController characterController)
    {
        float screenEdgesOffset = 1;
        Vector2 resurrectPos = characterController.targetResurrectPos;

        bool isWithinBounds =
            resurrectPos.x > minCameraCorner.x + screenEdgesOffset &&
            resurrectPos.x < maxCameraCorner.x - screenEdgesOffset &&
            resurrectPos.y > minCameraCorner.y && // Character can "walk" on the camera's lower bound
            resurrectPos.y < maxCameraCorner.y - 3f; // Character cannot resurrect higher because it would pass through the camera's upper bound

        return isWithinBounds;
    }

    public bool IsCharacterOnScreen(CharacterController characterController)
    {
        Vector2 screenEdgesOffset = new Vector2(0.5f, 1f);
        Vector2 characterPos = characterController.characterCenter;

        bool isOutOfBounds =
            characterPos.x < minCameraCorner.x + screenEdgesOffset.x ||
            characterPos.x > maxCameraCorner.x - screenEdgesOffset.x ||
            characterPos.y < minCameraCorner.y + screenEdgesOffset.y ||
            characterPos.y > maxCameraCorner.y - screenEdgesOffset.y;

        return !isOutOfBounds;
    }

    /*public void DisableCollidersUntilEveryoneIsAlive()
    {
        if (disableCollidersCoroutine != null)
            StopCoroutine(disableCollidersCoroutine);

        disableCollidersCoroutine = StartCoroutine(CoDisableCollidersUntilEveryoneIsAlive());
    }

    private IEnumerator CoDisableCollidersUntilEveryoneIsAlive()
    {
        SetCameraColliders(false);
        yield return new WaitUntil(() => gameManager.isEveryoneAlive);
        SetCameraColliders(true);
    }*/

    public void DisableCollidersUntilEveryoneIsOnScreen(bool isSoloMode)
    {
        if (disableCollidersCoroutine != null)
            StopCoroutine(disableCollidersCoroutine);

        disableCollidersCoroutine = StartCoroutine(CoDisableCollidersUntilEveryoneIsOnScreen(isSoloMode));
    }

    private IEnumerator CoDisableCollidersUntilEveryoneIsOnScreen(bool isSoloMode)
    {
        if (!isSoloMode)
            SetCameraColliders(false);
        SetCameraDeathTriggers(false);

        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => isEveryoneOnScreen);

        if (!isSoloMode)
            SetCameraColliders(true);
        SetCameraDeathTriggers(true);
    }

    #endregion
}
