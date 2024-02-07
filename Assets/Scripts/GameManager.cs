using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using System.Collections.Generic;

public class GameManager : InputListener {

	private static GameManager _Instance;
	public static GameManager Instance { get { return _Instance; } }

    [Header("PLAYERS")]
    [Space(10)]
    [SerializeField] [Range(1, maxPlayersCount)] private int startPlayersCount = 1;
    [SerializeField] [Range(1, maxPlayersCount)] private int playersCount = 1;
    [SerializeField] [Range(0, maxPlayersCount)] private int playersAlive = 1;
    private const int maxPlayersCount = 4;
    [Space]
    [SerializeField] private Transform playerGroup;
    [SerializeField] private CharacterController characterControllerPrefab;
    [Space]
    [SerializeField] private Material[] characterMaterials;

    [Header("SINGLETONS")]
    [Space(10)]
    [SerializeField] private CameraBehaviour _cameraBehaviour;
    private CameraBehaviour cameraBehaviour
    {
        get
        {
            if (!_cameraBehaviour)
                _cameraBehaviour = CameraBehaviour.Instance;

            return _cameraBehaviour;
        }
    }
    [SerializeField] private UI_MainCanvas _mainCanvas;
    private UI_MainCanvas mainCanvas
    {
        get
        {
            if (!_mainCanvas)
                _mainCanvas = UI_MainCanvas.Instance;

            return _mainCanvas;
        }
    }



    // Players
    private List<CharacterController> characterControllers = new List<CharacterController>();
    private List<Player> players = new List<Player>();

    // Events
    public delegate void DefaultCallback();
    public DefaultCallback OnPlayerAdded;
    public DefaultCallback OnPlayerRemoved;

    // Private & cache
    private float _deltaTime;
	public float deltaTime { get { return _deltaTime; } }
	private float _fixedDeltaTime;
	public float fixedDeltaTime { get { return _fixedDeltaTime; } }

    private int collectiblesCount;
    private int collectiblesMaxCount;

    private Checkpoint[] checkpoints;
    private int lastCheckpointID = -1;

    private bool _levelEnd = false;
    public bool levenEnd { get { return _levelEnd; } }
    private bool pause = false;

    public bool isSoloMode { get { return playersCount == 1; } }
    public bool isEveryoneAlive { get { return playersAlive == playersCount; } }
    public bool isEveryOneDead { get { return playersAlive == 0; } }

    #region UNITY_BASED

    private void Awake()
	{
		if (!_Instance)
		{
			_Instance = this;
		}
		else
		{
			Debug.LogError("Two instances of GameManager detected. Removing the latest one.");
			Destroy(gameObject);
			return;
		}
    }

    private void Start()
    {
        Init();
    }

    private void Update()
	{
		CacheData();
	}

    private void FixedUpdate()
    {
        FixedCacheData();
    }

    #endregion

    #region INIT_AND_MISC

    private void Init()
    {
        InitPlayersInputs();
        InitCharacters();
        SetCameraCharacters(true);

        //GetAllCollectibles();
        GetAllCheckpoints();
    }

    private void InitPlayersInputs()
    {
        players = new List<Player>();
        for (int i = 0; i < maxPlayersCount; i++)
        {
            players.Add(ReInput.players.GetPlayer(i));
            InitAllInputs(players[i]);
        }
    }

    private void InitCharacters()
    {
        CharacterController[] startPlayers = FindObjectsOfType<CharacterController>();

        foreach(CharacterController p in startPlayers)
        {
            Destroy(p.gameObject);
        }

        CharacterController[] characterControllersArray = new CharacterController[maxPlayersCount];
        for (int i = 0; i < maxPlayersCount; i++)
        {
            if (i >= startPlayersCount)
                break;

            CharacterController newCharacterController = Instantiate(characterControllerPrefab, playerGroup);
            characterControllersArray[i] = newCharacterController;
            newCharacterController.gameObject.name = "Player" + (i + 1);

            newCharacterController.SetRigidbodyPosition(new Vector2(i * 2 - (startPlayersCount - 1), 0));
            newCharacterController.SetPlayer(i);
            newCharacterController.SetStartPosition(newCharacterController.cachedTransform.position);

            newCharacterController.OnDeath += CharacterDied;
            newCharacterController.OnWaitForResurrect += CharacterWaitsForResurrect;
            newCharacterController.OnResurrect += CharacterResurrected;

            mainCanvas.playersPanel.SetPlayerPanel(newCharacterController, i);
        }
        characterControllers = new List<CharacterController>(characterControllersArray);

        playersCount = playersAlive = startPlayersCount;

        mainCanvas.playersPanel.CheckPlayerPanelsVisibility();
        mainCanvas.playersPanel.SetPlayerCamera(0);
    }

    private CharacterController GetCharacter(int playerID)
    {
        return characterControllers[playerID];
    }

    public void AddPlayer (int playerID)
    {
        // Cannot assign player 1
        if (playerID == 0)
            return;

        // Out of bounds
        if (playerID < 0 || playerID >= maxPlayersCount)
            return;

        // Character already created and assigned
        if (characterControllers[playerID] != null)
            return;

        CharacterController newCharacterController = Instantiate(characterControllerPrefab, playerGroup);
        newCharacterController.gameObject.name = "Player" + (playerID + 1);
        if (lastCheckpointID >= 0)
            newCharacterController.SetResurrectCheckpoint(checkpoints[lastCheckpointID]);
        newCharacterController.SetPlayer(playerID);
        newCharacterController.SetStartPosition(newCharacterController.cachedTransform.position);
        newCharacterController.Resurrect(characterControllers[0].cachedTransform.position);

        newCharacterController.OnDeath += CharacterDied;
        newCharacterController.OnWaitForResurrect += CharacterWaitsForResurrect;
        newCharacterController.OnResurrect += CharacterResurrected;

        mainCanvas.playersPanel.SetPlayerPanel(newCharacterController, playerID);

        characterControllers[playerID] = newCharacterController;

        SetCameraCharacters();

        playersCount++;
        playersAlive++;

        mainCanvas.playersPanel.CheckPlayerPanelsVisibility();

        OnPlayerAdded();
    }

    public void RemovePlayer(int playerID)
    {
        // Cannot delete player 1
        if (playerID == 0)
            return;

        // Out of bounds
        if (playerID < 0 || playerID >= maxPlayersCount)
            return;

        // Character already created and assigned
        if (characterControllers[playerID] == null)
            return;

        characterControllers[playerID].UnsubscribeAllInputs(characterControllers[playerID].GetPlayer());

        characterControllers[playerID].OnDeath -= CharacterDied;
        characterControllers[playerID].OnWaitForResurrect -= CharacterWaitsForResurrect;
        characterControllers[playerID].OnResurrect -= CharacterResurrected;

        if (!characterControllers[playerID].isDead)
            playersAlive--;

        Destroy(characterControllers[playerID].gameObject);
        characterControllers[playerID] = null;

        SetCameraCharacters();

        playersCount--;

        OnPlayerRemoved();
    }

    private Player GetPlayer(int playerID)
    {
        return players[playerID];
    }

    private void SetCameraCharacters(bool gameStart = false)
    {
        cameraBehaviour.SetCharacters(GetAliveCharactersList(), gameStart);
    }

    private List<CharacterController> GetAliveCharactersList()
    {
        List<CharacterController> cleanCharacterControllersList = new List<CharacterController>(characterControllers);
        cleanCharacterControllersList.RemoveAll(CharacterController => CharacterController == null || CharacterController.isDead);

        return cleanCharacterControllersList;
    }

    private void CacheData()
	{
		_deltaTime = Time.deltaTime;
	}

	private void FixedCacheData ()
	{
		_fixedDeltaTime = Time.fixedDeltaTime;
	}

    //public void GetAllCollectibles()
    //{
    //    collectiblesMaxCount = FindObjectsOfType<Collectible>().Length;

    //    if (mainCanvas)
    //        mainCanvas.SetCollectiblesText(collectiblesCount, collectiblesMaxCount);
    //}

    //public void CollectibleCollected()
    //{
    //    collectiblesCount++;
    //    mainCanvas.SetCollectiblesText(collectiblesCount, collectiblesMaxCount);
    //    mainCanvas.CollectibleCollected();

    //    if (collectiblesCount == collectiblesMaxCount)
    //        LevelEnd();
    //}

    #endregion

    #region INPUTS

    protected override void GetButtonDown(InputActionEventData data)
    {
        // All inputs locked
        if (lockAllInputs)
            return;

        switch (data.actionName)
        {
            case "Start":
                if (GetCharacter(data.playerId) == null) // Player not loaded yet
                {
                    if (pause)
                        break;

                    AddPlayer(data.playerId);
                }
                else
                {
                    SetPause();
                }
                break;

            case "LB":
                QuitGame();
                break;

            case "RB":
                RestartScene();
                break;
        }

        base.GetButtonDown(data);
    }

    #endregion

    #region BEHAVIOUR

    public void SetPause()
    {
        if (_levelEnd)
        {
            RestartScene();
            return;
        }

        pause = !pause;

        Time.timeScale = pause ? 0 : 1;

        mainCanvas.SetPause(pause);
        cameraBehaviour.SetBlur(pause);
    }

    private void LevelEnd()
    {
        _levelEnd = true;

        mainCanvas.LevelEnd();
        cameraBehaviour.SetBlur(true);
    }

    private void GetAllCheckpoints()
    {
        checkpoints = FindObjectsOfType<Checkpoint>();

        for (int i = 0; i < checkpoints.Length; i++)
            checkpoints[i].SetCheckpointID(i);

        /*if (checkpoints.Length > 0)
            SetCheckpoints(0);*/
    }

    public void SetCheckpoints(int newCheckpointID)
    {
        // Set all checkpoints state
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (i == newCheckpointID)
                continue;

            checkpoints[i].DisableCheckpoint();
        }

        // Set new resurrect position to all characters
        for (int i = 0; i < characterControllers.Count; i++)
        {
            if (!characterControllers[i])
                continue;

            characterControllers[i].SetResurrectCheckpoint(checkpoints[newCheckpointID]);
        }

        lastCheckpointID = newCheckpointID;
    }

    public CharacterController GetPlayerAlive()
    {
        for (int i = 0; i < characterControllers.Count; i++)
        {
            if (characterControllers[i] && !characterControllers[i].isDead)
            {
                return characterControllers[i];
            }
        }

        return null;
    }

    private void CharacterDied(CharacterController characterController)
    {
        playersAlive--;

        mainCanvas.playersPanel.EnablePlayerPanel(characterController.playerID, false);
    }

    private void CharacterWaitsForResurrect(CharacterController characterController)
    {
        if (isSoloMode || isEveryOneDead) // Everyone is dead: force resurrect the first character and disable camera colliders until everyone is back within screen bounds
        {
            if (!isSoloMode)
                characterControllers[0].CancelWaitForResurrect();
            cameraBehaviour.DisableCollidersUntilEveryoneIsOnScreen(isSoloMode);
            return;
        }

        if (cameraBehaviour.IsResurrectPositionInsideCameraBounds(characterController))
            return;

        SetCameraCharacters();
    }

    private void CharacterResurrected(CharacterController characterController)
    {
        playersAlive++;

        SetCameraCharacters();

        mainCanvas.playersPanel.EnablePlayerPanel(characterController.playerID, true);
    }

    public Material GetPlayerMaterial(int materialID)
    {
        if (characterMaterials[materialID])
            return characterMaterials[materialID];
        else
            return null;
    }

    #endregion

    #region SCENE_MANAGEMENT

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    #endregion
}
