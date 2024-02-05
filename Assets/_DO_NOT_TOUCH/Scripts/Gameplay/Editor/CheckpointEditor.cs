using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
[CustomEditor(typeof(Checkpoint))]
public class CheckpointEditor : Editor {

    static Checkpoint[] checkpoints = new Checkpoint[0];

    static CheckpointEditor()
    {
        EditorSceneManager.sceneOpened += FindCheckpoints;
        EditorApplication.playModeStateChanged += FindCheckpoints;
        EditorApplication.hierarchyChanged += FindCheckpoints;

        //EditorApplication.update += EditorUpdate;
    }

    static void FindCheckpoints(Scene scene, OpenSceneMode mode)
    {
        checkpoints = FindObjectsOfType<Checkpoint>();
        //SetCheckpointsID();
    }

    static void FindCheckpoints(PlayModeStateChange mode)
    {
        if (mode != PlayModeStateChange.EnteredEditMode)
            return;

        checkpoints = FindObjectsOfType<Checkpoint>();
        //SetCheckpointsID();
    }

    static void FindCheckpoints()
    {
        checkpoints = FindObjectsOfType<Checkpoint>();
        //SetCheckpointsID();
    }

    /*static void SetCheckpointsID()
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].SetCheckpointID(i);
        }
    }*/
}
