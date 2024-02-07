using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Checkpoint : MonoBehaviour {

    private CapsuleCollider _collider;
    public CapsuleCollider collider
    {
        get
        {
            if (!_collider)
                _collider = GetComponent<CapsuleCollider>();

            return _collider;
        }
    }

    [SerializeField] private ParticleSystem mainCheckpointFX;
    [SerializeField] private ParticleSystem enableCheckpointFX;
    [SerializeField] private Color checkpointEnabledColor = Color.green;
    [SerializeField] private Color checkpointDisabledColor = Color.blue;

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

    // Cache
    private CharacterController characterController;
    private int checkPointID = 0;
    private bool checkpointEnabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!checkpointEnabled)
        {
            characterController = other.GetComponent<CharacterController>();
            EnableCheckpoint();
        }
    }

    public void SetCheckpointID(int newID)
    {
        checkPointID = newID;
    }

    private void EnableCheckpoint()
    {
        enableCheckpointFX.Play();
        EditParticlesColor(checkpointEnabledColor);

        gameManager.SetCheckpoints(checkPointID);

        checkpointEnabled = true;
    }

    public void DisableCheckpoint()
    {
        EditParticlesColor(checkpointDisabledColor);

        checkpointEnabled = false;
    }

    private void EditParticlesColor (Color newColor)
    {
        ParticleSystem.MainModule psmm = mainCheckpointFX.main;
        psmm.startColor = newColor;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[mainCheckpointFX.particleCount];
        int particlesCount = mainCheckpointFX.GetParticles(particles);

        // Edit particles color on-the-fly
        for (int i = 0; i < particlesCount; i++)
        {
            particles[i].startColor = newColor;
        }
        mainCheckpointFX.SetParticles(particles, particlesCount);
    }

    public Vector2 GetCheckpointPosition()
    {
        return transform.position;
    }
}
