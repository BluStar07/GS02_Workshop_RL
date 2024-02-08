using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCreator : MonoBehaviour
{
    //Scene data once scene is set
    public class SceneSetData
    {
        public int sceneIndex;
        public Vector2Int gridPosition;
    }
    //Default scene data
    [System.Serializable]
    public class SceneData
    {
        [Tooltip("Name of the scene in asset, must match EXACTLY")]
        public string sceneName;
        [Tooltip("Doors positions in the scene, in order: Left, Right, Up, Down")]
        public List<DoorBehaviour.DoorPositions> doors = new List<DoorBehaviour.DoorPositions>();
        [Tooltip("Percentage of chance to spawn the room, from 1 to 100%")]
        [Range(1f, 100f)]
        public int dropRate = 50;
        [Tooltip("Number of time a room should spawn between x and y number, keep 0 if normal room")]
        public Vector2Int spawnNumber = new Vector2Int(0, 0);
        [Tooltip("Spawn this room only after a certain number of other rooms, keep 0 if should spawn randomly")]
        public int spawnAfterIndex = 0;
        public SceneSetData sceneSetData;
    }

    [SerializeField] private List<SceneData> sceneToLoad = new List<SceneData>();
    [SerializeField] private DoorBehaviour firstDoor;
    public Vector2Int grid = Vector2Int.one;
    [SerializeField] private Vector2Int firstPos = Vector2Int.zero;
    [SerializeField] private int seed;
    [SerializeField] private List<SceneData> sceneLoaded = new List<SceneData>();

    private List<Vector2Int> takenSlot = new List<Vector2Int>();
    private int indexRoom = 0;
    private int indexToExclude = -1;

    public static SceneCreator instance;

    private void Awake()
    {
        if (instance == null || instance != this) instance = this;
    }

    private void Start()
    {
        //Set current scene as first scene
        SceneData firstScene = new SceneData();
        firstScene.sceneSetData = new SceneSetData();
        firstScene.sceneName = SceneManager.GetActiveScene().name;
        firstScene.doors.Add(firstDoor.doorPositions);
        firstScene.sceneSetData.gridPosition = firstPos;
        sceneLoaded.Add(firstScene);
        takenSlot.Add(firstScene.sceneSetData.gridPosition);

        if (seed != 0)
        {
            Random.InitState(seed);
        }
        StartCoroutine(LoadScenes());
    }

    private IEnumerator LoadScenes()
    {
        //Set HUD scene
        yield return StartCoroutine(LoadScene("ScenesManager"));
        yield return null;
        HUD_MapBehaviour.instance.SetGrid();
        HUD_MapBehaviour.instance.lastRoom = sceneLoaded[0].sceneSetData.gridPosition;
        HUD_MapBehaviour.instance.SetActiveRoom(sceneLoaded[0].sceneSetData.gridPosition);

        //Load rooms
        yield return StartCoroutine(LoadRoomsScene());
        yield return null;

        //Generation over
        //Set first door
        firstDoor.nextRoomPos = sceneLoaded[1].sceneSetData.gridPosition;
        firstDoor.sceneToLoadIndex = sceneLoaded[1].sceneSetData.sceneIndex;

        //Set remaining doors
        for (int i = 1; i < sceneLoaded.Count; i++)
        {
            SceneData rightScene = null;
            SceneData leftScene = null;
            SceneData upScene = null;
            SceneData downScene = null;
            //Check every room to find neighborg and apply correct data
            for (int j = 0; j < sceneLoaded.Count; j++)
            {
                if (sceneLoaded[j].sceneSetData.gridPosition == sceneLoaded[i].sceneSetData.gridPosition + new Vector2Int(1, 0))
                {
                    rightScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].sceneSetData.gridPosition == sceneLoaded[i].sceneSetData.gridPosition + new Vector2Int(-1, 0))
                {
                    leftScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].sceneSetData.gridPosition == sceneLoaded[i].sceneSetData.gridPosition + new Vector2Int(0, -1))
                {
                    upScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].sceneSetData.gridPosition == sceneLoaded[i].sceneSetData.gridPosition + new Vector2Int(0, 1))
                {
                    downScene = sceneLoaded[j];
                }
            }
            //Set doors data
            SceneManager.GetSceneAt(sceneLoaded[i].sceneSetData.sceneIndex).GetRootGameObjects()[0].transform.GetComponentInChildren<ManagerScene>().SetRemainingDoors(rightScene, leftScene, upScene, downScene);
            //Debug delay
            yield return null;
        }
    }

    private IEnumerator LoadRoomsScene()
    {
        //For each scene
        for (int i = 0; i < sceneLoaded.Count; i++)
        {
            //For each doors of scene
            for (int j = 0; j < sceneLoaded[i].doors.Count; j++)
            {
                if (!CheckIfRoomExist(sceneLoaded[i], sceneLoaded[i].doors[j]))
                {
                    //Get a room prefab
                    SceneData sceneToCreate = null;

                    int tryRoom = 0;
                    //Recheck to get room if null
                    while (sceneToCreate == null && tryRoom < 20)
                    {
                        yield return null;
                        tryRoom++;
                        sceneToCreate = GetCorrectScene(sceneLoaded[i], sceneLoaded[i].doors[j]);
                    }

                    //if can keep generate
                    if (sceneToCreate != null && sceneToCreate.sceneName.Length > 0 && sceneLoaded.Count < (grid.x + 1) * (grid.y + 1))
                    {
                        seed += sceneToCreate.GetHashCode();
                        yield return StartCoroutine(LoadScene(sceneToCreate.sceneName));
                        yield return null;

                        SceneData newSceneData = new SceneData();
                        newSceneData.sceneSetData = new SceneSetData();
                        newSceneData.sceneName = sceneToCreate.sceneName;
                        newSceneData.sceneSetData.gridPosition = GetPosition(sceneLoaded[i], sceneLoaded[i].doors[j]);
                        takenSlot.Add(newSceneData.sceneSetData.gridPosition);
                        HUD_MapBehaviour.instance.SetRoom(takenSlot[takenSlot.Count - 1]);
                        newSceneData.sceneSetData.sceneIndex = indexRoom;
                        newSceneData.doors = sceneToCreate.doors;
                        sceneLoaded.Add(newSceneData);

                        //Destroy doors leading to grid edge
                        SceneManager.GetSceneAt(indexRoom).GetRootGameObjects()[0].transform.GetComponentInChildren<ManagerScene>().SetFirstDoors(indexRoom, newSceneData.sceneSetData, grid);

                        //Hide Room
                        DisableCurrentScene(SceneManager.GetSceneAt(indexRoom));
                    }
                }
            }
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        indexRoom++;
        while (!SceneManager.GetSceneAt(indexRoom).isLoaded)
        {
            yield return null;
        }
    }

    private int GetRandomIndex()
    {
        List<int> possibleIndex = new List<int>();
        for (int i = 0; i < sceneToLoad.Count; i++)
        {
            possibleIndex.Add(i);
        }
        if (indexToExclude >= 0 && sceneToLoad.Count > 1)
        {
            possibleIndex.RemoveAt(indexToExclude);
        }
        return possibleIndex[Random.Range(0, possibleIndex.Count)];
    }

    //Get correct scene
    private SceneData GetCorrectScene(SceneData lastRoom, DoorBehaviour.DoorPositions lastDoor)
    {
        int indexSceneToLoad = GetRandomIndex();
        SceneData room = sceneToLoad[indexSceneToLoad];

        //Check percentage
        if (Random.value > (float)(room.dropRate / 100f))
        {
            indexToExclude = indexSceneToLoad;
            return null;
        }

        //Check if should spawn after a certain number of room
        if (room.spawnAfterIndex > 0)
        {
            if (indexRoom <= room.spawnAfterIndex)
            {
                indexToExclude = indexSceneToLoad;
                return null;
            }
        }

        switch (lastDoor)
        {
            case DoorBehaviour.DoorPositions.right:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.left) && lastRoom.sceneSetData.gridPosition.x + 1 <= grid.x)
                {
                    indexToExclude = -1;
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.left:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.right) && lastRoom.sceneSetData.gridPosition.x - 1 >= 0)
                {
                    indexToExclude = -1;
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.up:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.down) && lastRoom.sceneSetData.gridPosition.y + 1 <= grid.y)
                {
                    indexToExclude = -1;
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.down:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.up) && lastRoom.sceneSetData.gridPosition.y - 1 >= 0)
                {
                    indexToExclude = -1;
                    return room;
                }
                break;
            default:
                break;
        }
        return null;
    }

    private bool CheckIfRoomExist(SceneData lastRoom, DoorBehaviour.DoorPositions lastDoor)
    {
        switch (lastDoor)
        {
            case DoorBehaviour.DoorPositions.right:
                return takenSlot.Contains(lastRoom.sceneSetData.gridPosition + new Vector2Int(1, 0));
            case DoorBehaviour.DoorPositions.left:
                return takenSlot.Contains(lastRoom.sceneSetData.gridPosition + new Vector2Int(-1, 0));
            case DoorBehaviour.DoorPositions.up:
                return takenSlot.Contains(lastRoom.sceneSetData.gridPosition + new Vector2Int(0, 1));
            case DoorBehaviour.DoorPositions.down:
                return takenSlot.Contains(lastRoom.sceneSetData.gridPosition + new Vector2Int(0, -1));
            default:
                return true;
        }
    }

    //Get position of an instantiate room
    private Vector2Int GetPosition(SceneData lastRoom, DoorBehaviour.DoorPositions lastDoor)
    {
        switch (lastDoor)
        {
            case DoorBehaviour.DoorPositions.right:
                return lastRoom.sceneSetData.gridPosition + new Vector2Int(1, 0);
            case DoorBehaviour.DoorPositions.left:
                return lastRoom.sceneSetData.gridPosition + new Vector2Int(-1, 0);
            case DoorBehaviour.DoorPositions.up:
                return lastRoom.sceneSetData.gridPosition + new Vector2Int(0, 1);
            case DoorBehaviour.DoorPositions.down:
                return lastRoom.sceneSetData.gridPosition + new Vector2Int(0, -1);
            default:
                return new Vector2Int(0, 0);
        }
    }

    //Hide scene when room is ready
    private void DisableCurrentScene(Scene scene)
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();

        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(false);
        }
    }
}
