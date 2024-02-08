using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneCreator;

public class SceneCreator : MonoBehaviour
{
    [System.Serializable]
    public class SceneData
    {
        public string sceneName;
        public int sceneIndex;
        public Vector2Int gridPosition;
        public List<DoorBehaviour.DoorPositions> doors = new List<DoorBehaviour.DoorPositions>();
    }

    [SerializeField] private List<SceneData> sceneToLoad = new List<SceneData>();
    [SerializeField] private SceneData lastScene;
    [SerializeField] private List<SceneData> sceneLoaded = new List<SceneData>();
    [SerializeField] private DoorBehaviour firstDoor;
    private int indexRoom = 1;
    [SerializeField] private Vector2Int grid = Vector2Int.one;
    private List<Vector2Int> takenSlot = new List<Vector2Int>();

    private void Start()
    {
        sceneLoaded.Add(lastScene);
        takenSlot.Add(lastScene.gridPosition);
        HUD_MapBehaviour.instance.SetRoom(takenSlot[takenSlot.Count - 1]);

        StartCoroutine(LoadScenes());
    }

    private IEnumerator LoadScenes()
    {
        yield return StartCoroutine(LoadScene());

        //Debug delay
        yield return null;

        //Generation over
        firstDoor.nextRoomPos = sceneLoaded[1].gridPosition;
        firstDoor.sceneToLoadIndex = sceneLoaded[1].sceneIndex;
        HUD_MapBehaviour.instance.lastRoom = sceneLoaded[0].gridPosition;
        HUD_MapBehaviour.instance.SetActiveRoom(sceneLoaded[0].gridPosition);

        for (int i = 0; i < sceneLoaded.Count; i++)
        {
            int index = 0;
            for (int j = 0; j < takenSlot.Count; j++)
            {
                if (takenSlot[j] == sceneLoaded[i].gridPosition)
                {
                    index++;
                }
            }
            if (index > 1)
            {
                Debug.LogError(sceneLoaded[i].gridPosition + " " + index + " times in loaded scenes");
            }
        }

        for (int i = 1; i < sceneLoaded.Count; i++)
        {
            SceneData rightScene = null;
            SceneData leftScene = null;
            SceneData upScene = null;
            SceneData downScene = null;
            for (int j = 0; j < sceneLoaded.Count; j++)
            {
                if (sceneLoaded[j].gridPosition == sceneLoaded[i].gridPosition + new Vector2Int(1, 0))
                {
                    rightScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].gridPosition == sceneLoaded[i].gridPosition + new Vector2Int(-1, 0))
                {
                    leftScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].gridPosition == sceneLoaded[i].gridPosition + new Vector2Int(0, -1))
                {
                    upScene = sceneLoaded[j];
                }
                if (sceneLoaded[j].gridPosition == sceneLoaded[i].gridPosition + new Vector2Int(0, 1))
                {
                    downScene = sceneLoaded[j];
                }
            }
            SceneManager.GetSceneAt(sceneLoaded[i].sceneIndex).GetRootGameObjects()[0].transform.GetComponentInChildren<ManagerScene>().SetRemainingDoors(rightScene, leftScene, upScene, downScene);
            yield return null;
        }
    }

    private IEnumerator LoadScene()
    {
        for (int i = 0; i < sceneLoaded.Count; i++)
        {
            for (int j = 0; j < sceneLoaded[i].doors.Count; j++)
            {
                SceneData sceneToCreate = GetCorrectScene(sceneLoaded[i], sceneLoaded[i].doors[j]);
                int rogueTry = 0;
                while (sceneToCreate == null && rogueTry < 9)
                {
                    sceneToCreate = GetCorrectScene(sceneLoaded[i], sceneLoaded[i].doors[j]);
                    rogueTry++;
                    yield return null;
                }

                if (sceneToCreate != null && sceneToCreate.sceneName.Length > 0 && sceneLoaded.Count < (grid.x + 1) * (grid.y + 1))
                {
                    SceneManager.LoadSceneAsync(sceneToCreate.sceneName, LoadSceneMode.Additive);

                    indexRoom++;
                    while (!SceneManager.GetSceneAt(indexRoom).isLoaded)
                    {
                        yield return null;
                    }
                    yield return null;

                    SceneData newSceneData = new SceneData();
                    newSceneData.sceneName = sceneToCreate.sceneName;
                    newSceneData.gridPosition = GetPosition(sceneLoaded[i], sceneLoaded[i].doors[j]);
                    takenSlot.Add(newSceneData.gridPosition);
                    HUD_MapBehaviour.instance.SetRoom(takenSlot[takenSlot.Count - 1]);
                    newSceneData.sceneIndex = indexRoom;
                    newSceneData.doors = sceneToCreate.doors;
                    sceneLoaded.Add(newSceneData);

                    //Set doors
                    SceneManager.GetSceneAt(indexRoom).GetRootGameObjects()[0].transform.GetComponentInChildren<ManagerScene>().SetFirstDoors(indexRoom, newSceneData, grid);

                    //Hide Room
                    DisableCurrentScene(SceneManager.GetSceneAt(indexRoom));

                    lastScene = newSceneData;
                }
            }
        }
    }

    private SceneData GetCorrectScene(SceneData lastRoom, DoorBehaviour.DoorPositions lastDoor)
    {
        SceneData room = sceneToLoad[Random.Range(0, sceneToLoad.Count)];

        switch (lastDoor)
        {
            case DoorBehaviour.DoorPositions.right:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.left) && lastRoom.gridPosition.x + 1 <= grid.x)
                {
                    if (takenSlot.Contains(lastRoom.gridPosition + new Vector2Int(1, 0)))
                    {
                        return null;
                    }
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.left:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.right) && lastRoom.gridPosition.x - 1 >= 0)
                {
                    if (takenSlot.Contains(lastRoom.gridPosition + new Vector2Int(-1, 0)))
                    {
                        return null;
                    }
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.up:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.down) && lastRoom.gridPosition.y + 1 <= grid.y)
                {
                    if (takenSlot.Contains(lastRoom.gridPosition + new Vector2Int(0, 1)))
                    {
                        return null;
                    }
                    return room;
                }
                break;
            case DoorBehaviour.DoorPositions.down:
                if (room.doors.Contains(DoorBehaviour.DoorPositions.up) && lastRoom.gridPosition.y - 1 >= 0)
                {
                    if (takenSlot.Contains(lastRoom.gridPosition + new Vector2Int(0, -1)))
                    {
                        return null;
                    }
                    return room;
                }
                break;
            default:
                break;
        }
        return null;
    }

    private Vector2Int GetPosition(SceneData lastRoom, DoorBehaviour.DoorPositions lastDoor)
    {
        switch (lastDoor)
        {
            case DoorBehaviour.DoorPositions.right:
                return lastRoom.gridPosition + new Vector2Int(1, 0);
            case DoorBehaviour.DoorPositions.left:
                return lastRoom.gridPosition + new Vector2Int(-1, 0);
            case DoorBehaviour.DoorPositions.up:
                return lastRoom.gridPosition + new Vector2Int(0, 1);
            case DoorBehaviour.DoorPositions.down:
                return lastRoom.gridPosition + new Vector2Int(0, -1);
            default:
                return new Vector2Int(0, 0);
        }
    }

    private void DisableCurrentScene(Scene scene)
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();

        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(false);
        }
    }
}
