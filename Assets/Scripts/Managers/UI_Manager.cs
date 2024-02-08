using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    private Camera camera;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        camera = Camera.main;
        this.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
    }
}
