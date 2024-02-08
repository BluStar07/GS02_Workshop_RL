using UnityEngine;
using UnityEngine.UI;

public class HUD_MapBehaviour : MonoBehaviour
{
    private Vector2Int gridSize;
    [SerializeField] private GridLayoutGroup grid;
    public Vector2Int lastRoom;

    public static HUD_MapBehaviour instance;

    private void Awake()
    {
        if (instance == null || instance != this) instance = this;
    }

    /// <summary>
    /// Set grid size
    /// </summary>
    public void SetGrid()
    {
        gridSize = SceneCreator.instance.grid;
        grid.cellSize = new Vector2((gridSize.x + 1) * 8, (gridSize.y + 1) * 8);
    }

    /// <summary>
    /// Set cell as room
    /// </summary>
    /// <param name="position"></param>
    public void SetRoom(Vector2Int position)
    {
        grid.transform.GetChild(GetIndexFromPos(position)).GetComponent<Image>().color = Color.black;
    }

    /// <summary>
    /// Set cell as current room
    /// </summary>
    /// <param name="position"></param>
    public void SetActiveRoom(Vector2Int position)
    {
        grid.transform.GetChild(GetIndexFromPos(lastRoom)).GetComponent<Image>().color = Color.grey;
        lastRoom = position;
        grid.transform.GetChild(GetIndexFromPos(position)).GetComponent<Image>().color = Color.red;
    }

    //Get cell from 2D grid position
    private int GetIndexFromPos(Vector2Int position)
    {
        int index = position.x;
        index += position.y * (gridSize.y + 1);
        return index;
    }
}
