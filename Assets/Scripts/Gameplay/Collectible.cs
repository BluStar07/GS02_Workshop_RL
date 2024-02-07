using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectible : MonoBehaviour {

    private SphereCollider _collider;
    private SphereCollider collider
    {
        get
        {
            if (!_collider)
                _collider = GetComponent<SphereCollider>();

            return _collider;
        }
    }

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private Light light;
    [Space]
    [SerializeField] private ParticleSystem collectedFXPrefab;

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

    // Cache
    private float lightIntensityDelay = 0.5f;
    private float startLightIntensity;

    private void Start()
    {
        Init();
    }

    private void OnTriggerEnter(Collider other)
    {
        DestroyCollectible();
    }

    private void Init()
    {
        if (light)
            startLightIntensity = light.intensity;
    }

    private void DestroyCollectible()
    {
        StartCoroutine(CoDestroyCollectible());
    }

    private IEnumerator CoDestroyCollectible()
    {
        collider.enabled = false;

        if (meshRenderer)
            meshRenderer.enabled = false;

        if (particleSystem)
            particleSystem.Stop();

        //gameManager.CollectibleCollected();

        Instantiate(collectedFXPrefab, transform);

        float t = 0f;
        while (t < lightIntensityDelay)
        {
            t += Time.deltaTime;
            float lerp = t / lightIntensityDelay;

            if (light)
                light.intensity = (1f - lerp) * startLightIntensity;

            yield return null;
        }

        if (!particleSystem)
            Destroy(gameObject);

        // Wait until the FX destroys itself
        while (particleSystem)
            yield return null;

        Destroy(gameObject);
    }
}
