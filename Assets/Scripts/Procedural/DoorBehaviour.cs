using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public class DoorBehaviour : MonoBehaviour
{
    public enum DoorPositions
    {
        right,
        left,
        up,
        down
    }

    public int sceneToLoadIndex;
    public int currentSceneIndex;
    public DoorPositions doorPositions;
    public Vector2Int nextRoomPos = new Vector2Int(0, 0);
    private bool canLoadScene = true;

    //Enter in door
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() && canLoadScene)
        {
            canLoadScene = false;
            LoadAndSwitchScene();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        canLoadScene = true;
    }

    //Load scene
    private void LoadAndSwitchScene()
    {
        Scene targetScene = SceneManager.GetSceneAt(sceneToLoadIndex);

        // Check if the scene exist and is already loaded
        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            DisableCurrentScene();
            ActivateSceneObject(targetScene);
        }
        else
        {
            Debug.LogError("Scene doesnt exist or is not loaded yet");
        }
    }

    //Show target scene
    private void ActivateSceneObject(Scene scene)
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(true);
        }
        HUD_MapBehaviour.instance.SetActiveRoom(nextRoomPos);
    }

    //Hide current scene
    private void DisableCurrentScene()
    {
        GameObject[] rootGameObjects = SceneManager.GetSceneAt(currentSceneIndex).GetRootGameObjects();

        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(false);
        }
    }
}
