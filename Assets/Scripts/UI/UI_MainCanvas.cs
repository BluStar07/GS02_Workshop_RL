using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MainCanvas : MonoBehaviour {

    private static UI_MainCanvas _Instance;
    public static UI_MainCanvas Instance { get { return _Instance; } }

    private Canvas _canvas;
    public Canvas canvas
    {
        get
        {
            if (!_canvas)
                _canvas = GetComponent<Canvas>();

            return _canvas;
        }
    }

    [Header("PANELS")]
    [Space(10)]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject levelEndPanel;
    [SerializeField] private UI_Panel_Players _playersPanel;
    public UI_Panel_Players playersPanel { get { return _playersPanel; } }

    [Header("COLLECTIBLES")]
    [Space(10)]
    [SerializeField] private TextMeshProUGUI collectiblesCountText;
    [SerializeField] private Animation collectiblesCountAnimation;
    [SerializeField] private TextMeshProUGUI collectiblesMaxCountText;
    [SerializeField] private ParticleSystem collectibleCollectedFX;

    private void Awake()
    {
        if (!_Instance)
        {
            _Instance = this;
        }
        else
        {
            Debug.LogError("Two instances of UI_MainCanvas detected. Removing the latest one.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        collectiblesCountText.SetText("0");
        levelEndPanel.SetActive(false);
    }

    public void SetCollectiblesText(int count, int maxCount)
    {
        collectiblesCountText.SetText(count.ToString());
        collectiblesMaxCountText.SetText("/" + maxCount);
    }

    public void CollectibleCollected ()
    {
        collectiblesCountAnimation.Stop();
        collectiblesCountAnimation.Play();
        collectibleCollectedFX.Play();
    }

    public void SetPause(bool pause)
    {
        pausePanel.SetActive(pause);
    }

    public void LevelEnd()
    {
        levelEndPanel.SetActive(true);
        //playersPanel.CheckPlayerPanelsVisibility();
    }
}
