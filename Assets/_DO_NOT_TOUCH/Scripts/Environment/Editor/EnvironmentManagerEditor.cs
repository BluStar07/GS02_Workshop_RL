using UnityEditor;

[CustomEditor(typeof(EnvironmentManager))]
[InitializeOnLoad]
public class EnvironmentManagerEditor : Editor {

    private static EnvironmentManager manager;

    private static void OnScene (SceneView sceneView)
    {
        if (!manager)
            manager = FindObjectOfType<EnvironmentManager>();

		if (manager)
			manager.ManageEnvironment();
    }

    static EnvironmentManagerEditor ()
    {
        SceneView.duringSceneGui += OnScene;
    }
}
