using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class CharacterBehaviour : MonoBehaviour {

	private Animator _animator;
	private Animator animator
	{
		get
		{
			if (!_animator)
				_animator = GetComponent<Animator>();

			return _animator;
		}
	}

	[Header("COMPONENTS")]
	[Space(10)]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Renderer characterMeshRenderer;


    [Header("BEHAVIOUR")]
	[Space(10)]
    [SerializeField] [Range(0f, 1f)] private float fastRunThresold = 0.5f;
    [SerializeField] private float heavyLandingFallVelocity = -2f;
    [SerializeField] private AnimationCurve groundTiltCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Space(10)]
    [SerializeField] private float hitLagStrength = 1f;

    [Header("UI")]
    [Space(10)]
    [SerializeField] private Transform _playerPanelAnchor;
    public Transform playerPanelAnchor { get { return _playerPanelAnchor; } }

    [Header("EFFECTS")]
    [Space(10)]
    [SerializeField] private ParticleSystem runSmoke;
    [SerializeField] private ParticleSystem uTurnSmokeLeft;
    [SerializeField] private ParticleSystem uTurnSmokeRight;
    [Space]
    [SerializeField] private ParticleSystem jumpSmokePrefab;
    [SerializeField] private ParticleSystem landSmokePrefab;
    [SerializeField] private ParticleSystem hitFXPrefab;
    [SerializeField] private ParticleSystem deathFXPrefab;
    [SerializeField] private ParticleSystem resurrectFXPrefab;

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

    // Private
    private float walkSpeed = 2.2f;
	private float runSpeed = 8f;
    private Vector2 velocity { get { return characterController.rigid.velocity; } }
    private float airDirection = 0f;
    private float runSpeedFactor = 1f;
    private float groundTilt = 0f;
    private int uTurnDirection;
    private bool hit;

    // Animator parameters
    private int ap_isMoving = Animator.StringToHash("isMoving");
    private int ap_speedLerp = Animator.StringToHash("speedLerp");
    private int ap_runSpeedFactor = Animator.StringToHash("runSpeedFactor");
    private int ap_airDirection = Animator.StringToHash("airDirection");
    private int ap_groundTilt = Animator.StringToHash("groundTilt");
    private int ap_yVelocity = Animator.StringToHash("yVelocity");
    private int ap_isGrounded = Animator.StringToHash("isGrounded");
    private int ap_isJumping = Animator.StringToHash("isJumping");

    private void Awake()
    {
        InitEvents();
    }

    private void Start()
    {
        Init();
        InitEvents();
    }

    private void Update()
	{
		GetValuesFromController();
		ApplyAnimatorParameters();

        UpdateFX();
        HitLag();
	}

    private void Init ()
    {
        ParticleSystem.EmissionModule runSmokeEM = runSmoke.emission;
        ParticleSystem.EmissionModule uTurnSmokeLeftEM = uTurnSmokeLeft.emission;
        ParticleSystem.EmissionModule uTurnSmokeRightEM = uTurnSmokeRight.emission;

        runSmokeEM.enabled = uTurnSmokeLeftEM.enabled = uTurnSmokeRightEM.enabled = false;
    }

    private void InitEvents()
    {
        characterController.OnJump += PlayJumpAnim;
        characterController.OnWallJump += PlayWallJumpAnim;
        characterController.OnLand += PlayLandAnim;
        characterController.OnUTurn += PlayUTurnAnim;
        characterController.OnResetCharacter += ResetAnimator;
        characterController.OnHit += StartHitLag;
        characterController.OnEndHitlag += EndHitLag;
        characterController.OnFreeze += PauseAnimator;
        characterController.OnUnfreeze += ResumeAnimator;
        characterController.OnDeath += Death;
        characterController.OnResurrect += Resurrect;
        characterController.OnSetPlayer += SetBodyMaterial;
    }

    private void GetValuesFromController ()
	{
        runSpeedFactor = Mathf.Lerp(characterController.minSpeed / walkSpeed, 1f, Mathf.InverseLerp(walkSpeed, runSpeed, characterController.targetSpeed));
        airDirection = Mathf.Lerp(
            airDirection,
            characterController.isGrounded ? 0f : (characterController.rigidbodyVelocity.x / characterController.maxSpeed * characterController.leftRight),
            4f * gameManager.deltaTime);

        GetGroundTilt(false);
	}

    private void GetGroundTilt (bool instantLerp)
    {
        float tiltDirection = Mathf.Sign(characterController.leftRight * characterController.groundHit.normal.x);
        float tiltLerp = Mathf.Clamp(-tiltDirection * characterController.groundAngle / characterController.maximumGroundAngle, -1f, 1f);
        groundTilt = Mathf.Lerp(groundTilt, groundTiltCurve.Evaluate(tiltLerp), instantLerp ? 1 : 8 * gameManager.deltaTime);
    }

	private void ApplyAnimatorParameters ()
	{
		animator.SetBool(ap_isMoving, characterController.isMoving);
        animator.SetBool(ap_isGrounded, characterController.isGrounded);
        animator.SetBool(ap_isJumping, characterController.isJumping);

        animator.SetFloat(ap_runSpeedFactor, runSpeedFactor);
        animator.SetFloat(ap_speedLerp, characterController.speedLerp);
        animator.SetFloat(ap_airDirection, airDirection);
        animator.SetFloat(ap_yVelocity, velocity.y);
        animator.SetFloat(ap_groundTilt, groundTilt);
    }

    private void UpdateFX ()
    {
        // Run smoke
        ParticleSystem.EmissionModule runSmokeEM = runSmoke.emission;
        runSmokeEM.enabled = characterController.uTurn ? false : (characterController.speedLerp == 1f && characterController.isGrounded);

        // U Turn smoke
        ParticleSystem.EmissionModule uTurnSmokeLeftEM = uTurnSmokeLeft.emission;
        uTurnSmokeLeftEM.enabled = characterController.uTurn && uTurnDirection == -1;

        ParticleSystem.EmissionModule uTurnSmokeRightEM = uTurnSmokeRight.emission;
        uTurnSmokeRightEM.enabled = characterController.uTurn && uTurnDirection == 1;
    }

    private void PlayJumpAnim(int jumpCount)
    {
        animator.Play(jumpCount == 0 ? "Jump" : (characterController.leftStickAxisLerped.x * characterController.leftRight >= 0f ? "DoubleJump_Front" : "DoubleJump_Back"), 0, 0);

        CreateAndSetFX(jumpSmokePrefab, characterController.transform.position);
    }

    private void PlayWallJumpAnim(RaycastHit wallHit)
    {
        animator.Play("WallJump", 0, 0);

        CreateAndSetFX(jumpSmokePrefab, wallHit.point, wallHit.normal);
    }

    private void PlayLandAnim (float landVelocity, RaycastHit groundHit)
    {
        if (landVelocity < heavyLandingFallVelocity)
            animator.Play("Landing", 0, 0);

        CreateAndSetFX(landSmokePrefab, groundHit.point, groundHit.normal);

        airDirection = 0f;
    }

    private void PlayUTurnAnim()
    {
        animator.Play("UTurn", 0, 0);
        uTurnDirection = -characterController.leftRight;

        GetGroundTilt(true);
    }

    private void ResetAnimator()
    {
        animator.SetBool(ap_isMoving, false);
        animator.SetBool(ap_isGrounded, false);
        animator.SetBool(ap_isJumping, false);

        animator.SetFloat(ap_runSpeedFactor, 0);
        animator.SetFloat(ap_speedLerp, 0);
        animator.SetFloat(ap_airDirection, 0);
        animator.SetFloat(ap_yVelocity, 0);

        animator.Play("Ilde", 0, 0);
    }

    private void PlayAnimNextFrame (string animName)
    {
        StartCoroutine(CoPlayAnimNextFrame(animName));
    }

    private IEnumerator CoPlayAnimNextFrame (string animName)
    {
        yield return new WaitForEndOfFrame();
        animator.Play(animName, 0, 0);
    }

    private void CreateAndSetFX(ParticleSystem fx, Vector3 pos)
    {
        Transform instantiatedFX = Instantiate(fx).transform;
        instantiatedFX.position = pos;
    }

    private void CreateAndSetFX(ParticleSystem fx, Vector3 pos, Vector3 lookForward)
    {
        Transform instantiatedFX = Instantiate(fx).transform;
        instantiatedFX.position = pos;
        instantiatedFX.rotation = Quaternion.LookRotation(lookForward);
    }

    private void StartHitLag()
    {
        hit = true;

        animator.Play("Hit", 0, 0);

        CreateAndSetFX(hitFXPrefab, characterController.characterCenter);
    }

    private void EndHitLag()
    {
        transform.localPosition = Vector3.zero;

        hit = false;
    }

    private void HitLag ()
    {
        if (!hit)
            return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, Random.insideUnitSphere * hitLagStrength, 24f * gameManager.deltaTime);
    }

    private void PauseAnimator ()
    {
        animator.speed = 0;
    }

    private void ResumeAnimator()
    {
        animator.speed = 1;
    }

    private void Death(CharacterController characterController)
    {
        characterMeshRenderer.enabled = false;

        CreateAndSetFX(deathFXPrefab, characterController.characterCenter);
    }

    private void Resurrect(CharacterController characterController)
    {
        characterMeshRenderer.enabled = true;

        CreateAndSetFX(resurrectFXPrefab, characterController.cachedTransform.position + new Vector3(0, -characterController.resurrectYOffset, 0));
        ResetAttributes();

        animator.Play("Fall", 0, 0);
    }

    private void ResetAttributes()
    {
        airDirection = 0f;
        runSpeedFactor = 0f;
        groundTilt = 0f;
        hit = false;
    }

    public void SetBodyMaterial(int playerID)
    {
        characterMeshRenderer.sharedMaterial = gameManager.GetPlayerMaterial(playerID);
    }

#if UNITY_EDITOR
#endif
}