using System.Collections.Generic;
using UnityEngine;
using static SceneCreator;

public class ManagerScene : MonoBehaviour
{
    [SerializeField] private List<DoorBehaviour> doors = new List<DoorBehaviour>();

    public void SetFirstDoors(int currentSceneIndex, SceneData currentScene, Vector2Int grid)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].currentSceneIndex = currentSceneIndex;
            switch (doors[i].doorPositions)
            {
                case DoorBehaviour.DoorPositions.right:
                    if (currentScene.gridPosition.x + 1 > grid.x)
                    {
                        doors[i].hasBeenSet = true;
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.left:
                    if (currentScene.gridPosition.x - 1 < 0)
                    {
                        doors[i].hasBeenSet = true;
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.up:
                    if (currentScene.gridPosition.y - 1 < 0)
                    {
                        doors[i].hasBeenSet = true;
                        Destroy(doors[i].transform.gameObject);
                        doors.RemoveAt(i);
                        i--;
                        break;
                    }
                    break;
                case DoorBehaviour.DoorPositions.down:
                    if (currentScene.gridPosition.y + 1 > grid.y)
                    {
                        doors[i].hasBeenSet = true;
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

    public void SetRemainingDoors(SceneData rightScene, SceneData leftScene, SceneData upScene, SceneData downScene)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            switch (doors[i].doorPositions)
            {
                case DoorBehaviour.DoorPositions.right:
                        doors[i].hasBeenSet = true;
                        doors[i].sceneToLoadIndex = rightScene.sceneIndex;
                        doors[i].nextRoomPos = rightScene.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.left:
                        doors[i].hasBeenSet = true;
                        doors[i].sceneToLoadIndex = leftScene.sceneIndex;
                        doors[i].nextRoomPos = leftScene.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.up:
                        doors[i].hasBeenSet = true;
                        doors[i].sceneToLoadIndex = upScene.sceneIndex;
                        doors[i].nextRoomPos = upScene.gridPosition;
                    break;
                case DoorBehaviour.DoorPositions.down:
                        doors[i].hasBeenSet = true;
                        doors[i].sceneToLoadIndex = downScene.sceneIndex;
                        doors[i].nextRoomPos = downScene.gridPosition;
                    break;
                default:
                    break;
            }
        }
    }
}
