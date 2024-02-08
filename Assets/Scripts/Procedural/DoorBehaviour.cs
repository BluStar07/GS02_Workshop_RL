using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
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
    private bool isLoadingScene;
    private bool canLoadScene = true;
    public DoorPositions doorPositions;
    public Vector2Int nextRoomPos = new Vector2Int(0,0);
    public bool hasBeenSet = false;

    private void Start()
    {
        //currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLoadingScene && other.GetComponent<CharacterController>() && canLoadScene)
        {
            canLoadScene = false;
            LoadAndSwitchScene();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        canLoadScene = true;
    }

    private void LoadAndSwitchScene()
    {
        // Check if the scene is already loaded
        Scene targetScene = SceneManager.GetSceneAt(sceneToLoadIndex);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            StartCoroutine(LoadSceneAndSwitchCoroutine());
        }
        else
        {
            StartCoroutine(SwitchSceneCoroutine(targetScene));
        }
    }

    private IEnumerator LoadSceneAndSwitchCoroutine()
    {
        isLoadingScene = true;

        yield return SceneManager.LoadSceneAsync(sceneToLoadIndex, LoadSceneMode.Additive);

        Scene targetScene = SceneManager.GetSceneAt(sceneToLoadIndex);

        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            DisableCurrentScene();
            ActivateSceneObject(targetScene);
        }
        else
        {
            Debug.LogError("Failed to load scene! Scene name: " + targetScene.name);
        }

        isLoadingScene = false;
    }

    private IEnumerator SwitchSceneCoroutine(Scene targetScene)
    {
        isLoadingScene = true;
        DisableCurrentScene();

        while (!targetScene.isLoaded)
        {
            yield return null;
        }

        ActivateSceneObject(targetScene);

        isLoadingScene = false;
    }

    private void ActivateSceneObject(Scene scene)
    {
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(true);
        }
        HUD_MapBehaviour.instance.SetActiveRoom(nextRoomPos);
    }

    private void DisableCurrentScene()
    {
        GameObject[] rootGameObjects = SceneManager.GetSceneAt(currentSceneIndex).GetRootGameObjects();

        if (rootGameObjects.Length > 0)
        {
            rootGameObjects[0].SetActive(false);
        }
    }
}
