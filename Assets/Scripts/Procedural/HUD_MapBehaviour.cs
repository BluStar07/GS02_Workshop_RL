using UnityEngine;
using UnityEngine.UI;

public class HUD_MapBehaviour : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    public Vector2Int lastRoom;

    public static HUD_MapBehaviour instance;

    private void Awake()
    {
        if (instance == null || instance != this) instance = this;
    }

    public void SetRoom(Vector2Int position)
    {
        grid.transform.GetChild(GetIndexFromPos(position)).GetComponent<Image>().color = Color.black;
    }
    public void SetActiveRoom(Vector2Int position)
    {
        grid.transform.GetChild(GetIndexFromPos(lastRoom)).GetComponent<Image>().color = Color.grey;
        lastRoom = position;
        grid.transform.GetChild(GetIndexFromPos(position)).GetComponent<Image>().color = Color.red;
    }

    private int GetIndexFromPos(Vector2Int position)
    {
        int index = position.x;
        index += position.y * 5;
        return index;
    }
}
