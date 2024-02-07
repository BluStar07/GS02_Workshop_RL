#if UNITY_EDITOR
using UnityEngine;

public class EnvironmentManager : MonoBehaviour {

	public EnviromentMode environmentMode;
	private EnviromentMode previousEnvironmentMode;
	[System.Serializable]
	public enum EnviromentMode
	{
		LevelDesign,
		Art
	}

	public GridManager gridManager;

    public Transform backgroundGroup;
    public Transform levelGroup;
	public Transform obstaclesGroup;
	public Transform checkpointsGroup;
	public Transform collectiblesGroup;
	public GameObject artGroup;
	public GameObject dynamicGroup;

    private MeshRenderer[] blocks;

#if UNITY_EDITOR
    private void OnValidate()
	{
		if (previousEnvironmentMode != environmentMode)
			SetEnvironmentMode();	
	}

	private void OnDrawGizmos()
	{
		ShowLevelDesignBoxes();
	}
#endif

	public void ManageEnvironment()
    {
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.hideFlags = HideFlags.NotEditable;

        if (!gridManager)
            return;

        if (!backgroundGroup || !levelGroup || !collectiblesGroup || !obstaclesGroup || !checkpointsGroup)
            return;

        backgroundGroup.position = levelGroup.position = obstaclesGroup.position = collectiblesGroup.position = checkpointsGroup.position = Vector3.zero;
		backgroundGroup.rotation = levelGroup.rotation = obstaclesGroup.rotation = collectiblesGroup.rotation = checkpointsGroup.rotation = Quaternion.identity;

		backgroundGroup.hideFlags = levelGroup.hideFlags = obstaclesGroup.hideFlags = collectiblesGroup.hideFlags = checkpointsGroup.hideFlags = HideFlags.NotEditable;

		SnapTransforms(levelGroup, 9);
		SnapTransforms(obstaclesGroup, 12);
		SnapTransforms(checkpointsGroup, 10, 1);
		SnapTransforms(collectiblesGroup, 10, 1);

		//SetDynamicGroup();
    }

	/*private void SetDynamicGroup ()
	{
		// Missing GameObject
		if (!dynamicGroup)
			return;

		dynamicGroup.layer = 9;

		for (int i = 0; i < dynamicGroup.transform.childCount; i++)
		{
			Transform child = dynamicGroup.transform.GetChild(i);

			child.gameObject.layer = child.GetComponent<Obstacle>() ? 10 : 9;

			if (!child.GetComponent<Collider>())
				child.gameObject.AddComponent<BoxCollider>();

			if (!child.GetComponent<MovingPlatform>())
				child.gameObject.AddComponent<MovingPlatform>();
		}
	}*/

	private void SnapTransforms(Transform parent, int layerIndex, float forceScale = 0)
	{
		//int children = parent.childCount;

		//for (int i = 0; i < children; i++)
		//{
		//	Transform child = parent.GetChild(i);

		//	if (layerIndex > -1)
		//		child.gameObject.layer = layerIndex;

		//	// Scaling
		//	Vector3 scale = child.localScale;
		//	if (forceScale > 0)
		//	{
		//		child.localScale = Vector3.one * forceScale;
		//	}
		//	else
		//	{
		//		Vector2 scaleOffset = new Vector2(scale.x % gridManager.unitSize, scale.y % gridManager.unitSize);

		//		scale.x += Mathf.InverseLerp(0f, gridManager.unitSize, scaleOffset.x) < 0.5f ? -scaleOffset.x : gridManager.unitSize - scaleOffset.x;
		//		scale.y += Mathf.InverseLerp(0f, gridManager.unitSize, scaleOffset.y) < 0.5f ? -scaleOffset.y : gridManager.unitSize - scaleOffset.y;
		//		scale.z = 2;

		//		child.localScale = scale;
		//	}


		//	// Position
		//	Vector3 pos = child.position;

		//	Vector2 posOffset = new Vector2(pos.x % gridManager.unitSize, pos.y % gridManager.unitSize);

		//	pos.x += Mathf.InverseLerp(0f, gridManager.unitSize, posOffset.x) < 0.5f ? -posOffset.x : gridManager.unitSize - posOffset.x;
		//	pos.y += Mathf.InverseLerp(0f, gridManager.unitSize, posOffset.y) < 0.5f ? -posOffset.y : gridManager.unitSize - posOffset.y;
		//	pos.z = 0;

		//	if (scale.x / gridManager.unitSize % 2f != 0) // Odd X scale
		//		pos.x -= gridManager.unitSize / 2f;
		//	if (scale.y / gridManager.unitSize % 2f != 0) // Odd Y scale
		//		pos.y -= gridManager.unitSize / 2f;

		//	child.position = pos;

		//	// Rotation
		//	child.rotation = Quaternion.identity;
		//}
	}

	public void SetEnvironmentMode ()
	{
		// Missing Transform(s)
		if (!artGroup || !levelGroup || !backgroundGroup)
			return;

		blocks = levelGroup.GetComponentsInChildren<MeshRenderer>();

        switch (environmentMode)
		{
			case EnviromentMode.LevelDesign:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i].enabled = true;
                artGroup.SetActive(false);
				backgroundGroup.gameObject.SetActive(true);
				break;

			case EnviromentMode.Art:
				for (int i = 0; i < blocks.Length; i++)
					blocks[i].enabled = false;
				artGroup.SetActive(true);
				backgroundGroup.gameObject.SetActive(false);
				break;
		}

		previousEnvironmentMode = environmentMode;
	}

	public void ShowLevelDesignBoxes ()
	{
		// No need to draw gizmos
		if (environmentMode == EnviromentMode.LevelDesign)
			return;

		for (int i = 0; i < blocks.Length; i++)
		{
			MeshRenderer block = blocks[i];

			Gizmos.color = new Color(1, 1, 0, 0.25f);
			Gizmos.DrawWireCube(block.transform.position, block.transform.localScale);
			Gizmos.color = new Color(1, 1, 0, 0.1f);
			Gizmos.DrawCube(block.transform.position, block.transform.localScale);
		}
	}
}
#endif
