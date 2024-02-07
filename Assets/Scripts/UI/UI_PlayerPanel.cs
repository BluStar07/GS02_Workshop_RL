using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerPanel : MonoBehaviour {

    private RectTransform _rectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }

    [SerializeField] private CharacterController _characterController;
    public CharacterController characterController { get { return _characterController; } }
    [SerializeField] private Vector2 anchorOffset;
    [SerializeField] private Image cameraIcon;

    // Singletons
    private CameraBehaviour _cameraBehaviour;
    private CameraBehaviour cameraBehaviour
    {
        get
        {
            if (!_cameraBehaviour)
                _cameraBehaviour = CameraBehaviour.Instance;

            return _cameraBehaviour;
        }
    }
    private UI_MainCanvas _mainCanvas;
    private UI_MainCanvas mainCanvas
    {
        get
        {
            if (!_mainCanvas)
                _mainCanvas = UI_MainCanvas.Instance;

            return _mainCanvas;
        }
    }

    private void LateUpdate()
    {
        SetPanelPosition();
    }

    public void SetCharacterBehaviour(CharacterController newCharacterController)
    {
        _characterController = newCharacterController;
    }

    private void SetPanelPosition()
    {
        if (!_characterController)
            return;

        Vector2 screenPosition =
            (Vector2)cameraBehaviour.cam.WorldToScreenPoint(
                characterController.cachedTransform.position + new Vector3(0, characterController.defaultColliderHeight, 0)) / mainCanvas.canvas.scaleFactor + anchorOffset;
        rectTransform.anchoredPosition = screenPosition;
    }

    public void SetPlayerCamera(bool showCameraIcon)
    {
        cameraIcon.enabled = showCameraIcon;
    }
}
