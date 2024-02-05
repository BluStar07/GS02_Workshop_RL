using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
[CustomEditor(typeof(Hitbox))]
public class HitboxEditor : Editor {

    static Hitbox[] hurtboxes = new Hitbox[0];

    static HitboxEditor()
    {
        EditorSceneManager.sceneOpened += FindHurtboxes;
        EditorApplication.playModeStateChanged += FindHurtboxes;
        EditorApplication.hierarchyChanged += FindHurtboxes;

        //EditorApplication.update += EditorUpdate;
    }

    static void FindHurtboxes(Scene scene, OpenSceneMode mode)
    {
        hurtboxes = FindObjectsOfType<Hitbox>();
        EditHurtboxes();
    }

    static void FindHurtboxes(PlayModeStateChange mode)
    {
        if (mode != PlayModeStateChange.EnteredEditMode)
            return;

        hurtboxes = FindObjectsOfType<Hitbox>();
        EditHurtboxes();
    }

    static void FindHurtboxes()
    {
        hurtboxes = FindObjectsOfType<Hitbox>();
        EditHurtboxes();
    }

    static void EditHurtboxes()
    {
        for (int i = 0; i < hurtboxes.Length; i++)
        {
            if (!hurtboxes[i])
                continue;

            Hitbox hurtbox = hurtboxes[i];

            hurtbox.gameObject.layer = 12;
            hurtbox.collider.isTrigger = true;
        }
    }
}
