using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_Players : MonoBehaviour {

    [SerializeField] private UI_PlayerPanel[] playerPanels;

    // Singletons
    private GameManager _gameManager;
    private GameManager gameManager
    {
        get
        {
            if (!_gameManager)
                _gameManager = GameManager.Instance;

            return _gameManager;
        }
    }

    public void SetAllPlayerPanels(CharacterController[] characterControllers)
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            playerPanels[i].SetCharacterBehaviour(characterControllers[i]);
        }
    }

    public void SetPlayerPanel(CharacterController characterController, int behaviourID)
    {
        if (behaviourID >= playerPanels.Length)
            return;

        playerPanels[behaviourID].SetCharacterBehaviour(characterController);
        EnablePlayerPanel(behaviourID, true);
    }

    public void EnablePlayerPanel(int panelID, bool enable)
    {
        enable = gameManager.isSoloMode || gameManager.levenEnd ? false : enable; // All player panels are not visible in solo mode
        playerPanels[panelID].gameObject.SetActive(enable);
    }

    public void CheckPlayerPanelsVisibility()
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            EnablePlayerPanel(i, playerPanels[i].characterController != null && !playerPanels[i].characterController.isDead);
        }
    }

    public void SetPlayerCamera(int playerID)
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            playerPanels[i].SetPlayerCamera(i == playerID);
        }
    }
}
