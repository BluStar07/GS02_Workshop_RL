using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor {

	static GameManager gameManager;

	static GameManagerEditor()
	{
        EditorSceneManager.sceneOpened += FindGameManager;
        EditorApplication.playModeStateChanged += FindGameManager;

        EditorApplication.update += EditorUpdate;
    }

	private void OnEnable()
	{
		gameManager = target as GameManager;

		Tools.hidden = true;

		LockTarget();
	}

	private void OnDisable()
	{
		Tools.hidden = false;

		LockTarget();
	}

    private void LockTarget ()
	{
        if (!gameManager)
            return;

		gameManager.transform.hideFlags = HideFlags.NotEditable;

		gameManager.gameObject.name = "GameManager";
		gameManager.transform.position = gameManager.transform.eulerAngles = Vector3.zero;
		gameManager.transform.localScale = Vector3.one;
	}

	static void FindGameManager(Scene scene, OpenSceneMode mode)
	{
		gameManager = FindObjectOfType<GameManager>();
    }

    static void FindGameManager(PlayModeStateChange mode)
    {
        if (mode != PlayModeStateChange.EnteredEditMode)
            return;

        gameManager = FindObjectOfType<GameManager>();
    }

    static void EditorUpdate()
	{
        // Don not execute in runtime
        if (Application.isPlaying)
            return;

        if (gameManager)
            gameManager.GetAllCollectibles();
    }
}
