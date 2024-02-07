#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GridManager : MonoBehaviour {

    [Header("UNITS")]
    [Space(10)]
    [Range(0.2f, 2f)]
    public float unitSize = 0.5f;
    public int gridSize = 32;
    [Range(0, 10)]
    public int unitsStep = 8;

    [Header("3D")]
    [Space(10)]
    public Material[] gridMaterials;

    [Header("GRID COLOR MANAGEMENT")]
    [Space(10)]
    public Color gridColor = new Color (1, 0, 0, 0.25f);
    public Color gridStepColor = Color.red;
    [Space(10)]
    public int minDistance = 50;
    public int maxDistance = 500;

	[Header("MISC")]
	[Space(10)]
	public bool displayGridInEditor = true;
    public bool displayGridInPlayMode = false;

    // Private
    private Color mainColor;
    private Color stepColor;

    private Plane zPlane = new Plane(Vector3.zero, Vector3.up, Vector3.right);

    private void OnValidate()
    {
        for (int i = 0; i < gridMaterials.Length; i++)
        {
            Material mat = gridMaterials[i];

            if (mat && mat.HasProperty("_UnitSize"))
                mat.SetFloat("_UnitSize", unitSize);
        }

        SetSnapSettings();
    }

    private void OnDrawGizmos()
    {
		if (Application.isPlaying)
		{
			if (!displayGridInPlayMode)
				return;
		}
		else
		{
			if (!displayGridInEditor)
				return;
		}

        if (unitSize == 0f)
            return;

        Transform cam = Camera.current.transform;

        float distance;
        Ray ray = new Ray(cam.position, cam.forward);
        zPlane.Raycast(ray, out distance); // Pos when forward hits z = 0
        Vector2 refPoint = (Vector2)(cam.position + cam.forward * distance) - Vector2.one * gridSize / 2f;

        Vector2 nearestStartPoint = new Vector2(refPoint.x - (refPoint.x % unitSize), refPoint.y - (refPoint.y % unitSize));
        Vector2 deltaPoint = nearestStartPoint - refPoint;

        float distanceAlphaFactor = Mathf.InverseLerp(maxDistance, minDistance, distance);

        mainColor = gridColor;
        mainColor.a *= distanceAlphaFactor;

        stepColor = gridStepColor;
        stepColor.a *= distanceAlphaFactor;

        // Vertical slices
        for (float x = refPoint.x; x < refPoint.x + gridSize; x += unitSize)
        {
            Handles.color = Gizmos.color = GetStepColor(x + deltaPoint.x);

            //Gizmos.DrawLine(new Vector2(x + deltaPoint.x, refPoint.y), new Vector2(x + deltaPoint.x, refPoint.y + gridSize));
            Handles.DrawLine(new Vector2(x + deltaPoint.x, refPoint.y), new Vector2(x + deltaPoint.x, refPoint.y + gridSize));
        }

        // Horizontal slices
        for (float y = refPoint.y; y < refPoint.y + gridSize; y += unitSize)
        {
            Handles.color = Gizmos.color = GetStepColor(y + deltaPoint.y);

            //Gizmos.DrawLine(new Vector2(refPoint.x, y + deltaPoint.y), new Vector2(refPoint.x + gridSize, y + deltaPoint.y));
            Handles.DrawLine(new Vector2(refPoint.x, y + deltaPoint.y), new Vector2(refPoint.x + gridSize, y + deltaPoint.y));
        }
    }

    private Color GetStepColor (float value)
    {
        return Mathf.Round(value / unitSize) % unitsStep == 0 ? stepColor : mainColor;
    }

    private void SetSnapSettings ()
    {
        EditorPrefs.SetFloat("MoveSnapX", unitSize);
        EditorPrefs.SetFloat("MoveSnapY", unitSize);
        EditorPrefs.SetFloat("ScaleSnap", unitSize);
    }
}
#endif
