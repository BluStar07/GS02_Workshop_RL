using System.Collections.Generic;
using UnityEngine;
using static SceneCreator;

public class ManagerScene : MonoBehaviour
{
    public List<DoorBehaviour> doors = new List<DoorBehaviour>();

    /// <summary>
    /// Destroy door if it leads to grid edges
    /// </summary>
    /// <param name="currentSceneIndex"></param>
    /// <param name="currentScene"></param>
    /// <param name="grid"></param>
    public void SetFirstDoors(int currentSceneIndex, SceneSetData currentScene, Vector2Int grid)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].currentSceneIndex = currentSceneIndex;
            switch (doors[i].doorPositions)
            {
                case DoorBehaviour.DoorPositions.right:
                    if (currentScene.gridPosition.x + 1 > grid.x)
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.left:
                    if (currentScene.gridPosition.x - 1 < 0)
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.up:
                    if (currentScene.gridPosition.y - 1 < 0)
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.down:
                    if (currentScene.gridPosition.y + 1 > grid.y)
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Set doors data, position and scene index to load
    /// </summary>
    /// <param name="rightScene"></param>
    /// <param name="leftScene"></param>
    /// <param name="upScene"></param>
    /// <param name="downScene"></param>
    public void SetRemainingDoors(SceneData rightScene, SceneData leftScene, SceneData upScene, SceneData downScene)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            switch (doors[i].doorPositions)
            {
                case DoorBehaviour.DoorPositions.right:
                    if (rightScene == null || !rightScene.doors.Contains(DoorBehaviour.DoorPositions.left))
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    doors[i].sceneToLoadIndex = rightScene.sceneSetData.sceneIndex;
                    doors[i].nextRoomPos = rightScene.sceneSetData.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.left:
                    if (leftScene == null || !leftScene.doors.Contains(DoorBehaviour.DoorPositions.right))
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    doors[i].sceneToLoadIndex = leftScene.sceneSetData.sceneIndex;
                    doors[i].nextRoomPos = leftScene.sceneSetData.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.up:
                    if (upScene == null || !upScene.doors.Contains(DoorBehaviour.DoorPositions.down))
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    doors[i].sceneToLoadIndex = upScene.sceneSetData.sceneIndex;
                    doors[i].nextRoomPos = upScene.sceneSetData.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.down:
                    if (downScene == null || !downScene.doors.Contains(DoorBehaviour.DoorPositions.up))
                    {
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    doors[i].sceneToLoadIndex = downScene.sceneSetData.sceneIndex;
                    doors[i].nextRoomPos = downScene.sceneSetData.gridPosition;
                    break;
                default:
                    break;
            }
        }
    }
}
