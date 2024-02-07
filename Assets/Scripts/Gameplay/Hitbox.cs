using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour {

    private Collider _collider;
    public Collider collider
    {
        get
        {
            if (!_collider)
                _collider = GetComponent<Collider>();

            return _collider;
        }
    }

    [SerializeField] private float hitLagDuration = 0.2f;

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

    private void OnTriggerEnter(Collider other)
    {
        CharacterController character = other.GetComponent<CharacterController>();

        if (character)
        {
            character.Hit();
        }
    }
}
