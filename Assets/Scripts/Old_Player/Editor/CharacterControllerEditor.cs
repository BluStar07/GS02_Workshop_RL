using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
[CustomEditor(typeof(CharacterController))]
public class CharacterControllerEditor : Editor
{
    static CharacterController[] characterControllers = new CharacterController[0];

    static CharacterControllerEditor()
    {
        EditorSceneManager.sceneOpened += FindCharacterControllers;
        EditorApplication.playModeStateChanged += FindCharacterControllers;
        EditorApplication.hierarchyChanged += FindCharacterControllers;

        EditorApplication.update += EditorUpdate;
    }

    static void FindCharacterControllers(Scene scene, OpenSceneMode mode)
    {
        characterControllers = FindObjectsOfType<CharacterController>();
    }

    static void FindCharacterControllers(PlayModeStateChange mode)
    {
        if (mode != PlayModeStateChange.EnteredEditMode)
            return;

        characterControllers = FindObjectsOfType<CharacterController>();
    }

    static void FindCharacterControllers()
    {
        characterControllers = FindObjectsOfType<CharacterController>();
    }

    static void EditorUpdate()
    {
        // Do not execute in runtime
        if (Application.isPlaying)
            return;

        for (int i = 0; i < characterControllers.Length; i++)
        {
            if (!characterControllers[i])
                continue;

            CharacterController c = characterControllers[i];

            float r = c.defaultColliderRadius;
            float h = c.defaultColliderHeight;

            if (h < 2 * r)
                h = 2 * r;

            if (2 * r > h)
                h = 2 * r;

            c.SetColliderParameters(r, h, h / 2f);
            c.UpdateColliderParameters();
        }
    }
}
