using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
[CustomEditor(typeof(CameraBehaviour))]
public class CameraBehaviourEditor : Editor {

	static CameraBehaviour cameraBehaviour;

    static Vector2 minFrame;
    static Vector2 maxFrame;

    static bool minFrameEdit;
    static bool maxFrameEdit;

    static CameraBehaviourEditor()
	{
		EditorSceneManager.sceneOpened += FindCameraBehaviour;
        EditorApplication.playModeStateChanged += FindCameraBehaviour;
        EditorApplication.hierarchyChanged += FindCameraBehaviour;

        EditorApplication.update += EditorUpdate;
	}

	private void OnEnable()
	{
		cameraBehaviour = target as CameraBehaviour;
	}

    static void FindCameraBehaviour(Scene scene, OpenSceneMode mode)
	{
		cameraBehaviour = FindObjectOfType<CameraBehaviour>();
	}

    static void FindCameraBehaviour(PlayModeStateChange mode)
    {
        if (mode != PlayModeStateChange.EnteredEditMode)
            return;

        cameraBehaviour = FindObjectOfType<CameraBehaviour>();
    }

    static void FindCameraBehaviour()
    {
        cameraBehaviour = FindObjectOfType<CameraBehaviour>();
    }

    static void EditorUpdate ()
	{
        if (Application.isPlaying)
            return;

        if (!cameraBehaviour)
            return;

        // Get components
        if (!cameraBehaviour.cam)
			cameraBehaviour.GetCamera();

		CharacterController[] characters = FindObjectsOfType<CharacterController>();
		if (characters.Length != cameraBehaviour.characters.Count)
			cameraBehaviour.GetCharacters();

        // Inspector frames edit
        if (minFrame.x != cameraBehaviour.minFrame.x)
        {
            cameraBehaviour.SetMinFrame(new Vector2(cameraBehaviour.minFrame.x, cameraBehaviour.minFrame.x / cameraBehaviour.cameraAspect));

            minFrameEdit = true;
            maxFrameEdit = false;
        }
        else if (minFrame.y != cameraBehaviour.minFrame.y)
        {
            cameraBehaviour.SetMinFrame(new Vector2(cameraBehaviour.minFrame.y * cameraBehaviour.cameraAspect, cameraBehaviour.minFrame.y));

            minFrameEdit = true;
            maxFrameEdit = false;
        }

        if (maxFrame.x != cameraBehaviour.maxFrame.x)
        {
            cameraBehaviour.SetMaxFrame(new Vector2(cameraBehaviour.maxFrame.x, cameraBehaviour.maxFrame.x / cameraBehaviour.cameraAspect));

            minFrameEdit = false;
            maxFrameEdit = true;
        }
        else if (maxFrame.y != cameraBehaviour.maxFrame.y)
        {
            cameraBehaviour.SetMaxFrame(new Vector2(cameraBehaviour.maxFrame.y * cameraBehaviour.cameraAspect, cameraBehaviour.maxFrame.y));

            minFrameEdit = false;
            maxFrameEdit = true;
        }

        // Update behaviour
        cameraBehaviour.GetCharactersMinMaxPosition();
        cameraBehaviour.SetCameraGroupPosition(true);
		cameraBehaviour.SetTargetZoomLerp();
		cameraBehaviour.Zoom(true);
        cameraBehaviour.UpdateCameraColliders();

		minFrame = cameraBehaviour.minFrame;
		maxFrame = cameraBehaviour.maxFrame;
	}
}
