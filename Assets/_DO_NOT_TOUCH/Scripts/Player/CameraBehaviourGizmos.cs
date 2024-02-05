#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public partial class CameraBehaviour : InputListener {

	private void OnDrawGizmos()
	{
        if (!drawGizmos)
            return;

		// Cache
		Quaternion gizmoRot = Quaternion.identity;

		// Barycenter
		Gizmos.color = Handles.color = Color.red;

		//Handles.SphereHandleCap(-1, charactersBarycenter, gizmoRot, 0.25f, EventType.Repaint);

		for (int i = 0; i < characters.Count; i++)
		{
            if (!characters[i])
                continue;

			Handles.SphereHandleCap(-1, characters[i].characterCenter, gizmoRot, 0.15f, EventType.Repaint);
			//Handles.DrawDottedLine(characters[i].characterCenter, charactersBarycenter, 3);
		}

		// Current position & framing
		Gizmos.color = Handles.color = Color.yellow;

		Handles.SphereHandleCap(-1, cachedTransform.position, gizmoRot, 0.15f, EventType.Repaint);
		Handles.SphereHandleCap(-1, cachedTransform.position - (Vector3)positionOffset, gizmoRot, 0.15f, EventType.Repaint);
		Handles.DrawDottedLine(cachedTransform.position, cachedTransform.position - (Vector3)positionOffset, 3);

		float frustumHeight = 2.0f * Mathf.Abs(cam.transform.position.z) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float frustumWidth = frustumHeight * cameraAspect;

        Vector2[] currentCorners = DrawFrameFromBox(cachedTransform.position, new Vector2(frustumWidth, frustumHeight));

		// Target framing
		Gizmos.color = Handles.color = Color.red;

        // Min frame
        Vector2[] minFrameCorners = DrawFrameFromBox(cachedTransform.position, minFrame);
        Handles.Label(minFrameCorners[2], "Min frame");

        // Max frame
        Vector2[] maxFrameCorners = DrawFrameFromBox(cachedTransform.position, maxFrame);
        Handles.Label(maxFrameCorners[2], "Max frame");

		// Frames in-between
		Handles.DrawDottedLine(minFrameCorners[0], maxFrameCorners[0], 3);
		Handles.DrawDottedLine(minFrameCorners[1], maxFrameCorners[1], 3);
		Handles.DrawDottedLine(minFrameCorners[2], maxFrameCorners[2], 3);
		Handles.DrawDottedLine(minFrameCorners[3], maxFrameCorners[3], 3);

        // World limits
        Gizmos.color = Handles.color = Color.black;

        DrawFrameFromMinMaxCorners(minWorldLimits, maxWorldLimits);

        // Characters frame
        Gizmos.color = Handles.color = Color.cyan;

        Vector2[] minCharactersFrameCorners = DrawFrameFromBox(cachedTransform.position, minCharactersFrame);
        Vector2[] maxCharactersFrameCorners = DrawFrameFromBox(cachedTransform.position, maxCharactersFrame);

        Handles.DrawDottedLine(minCharactersFrameCorners[0], maxCharactersFrameCorners[0], 3);
        Handles.DrawDottedLine(minCharactersFrameCorners[1], maxCharactersFrameCorners[1], 3);
        Handles.DrawDottedLine(minCharactersFrameCorners[2], maxCharactersFrameCorners[2], 3);
        Handles.DrawDottedLine(minCharactersFrameCorners[3], maxCharactersFrameCorners[3], 3);

        if (characters.Count < 2)
            return;

        // Characters spacing
        Gizmos.color = Handles.color = new Color(0, 1, 1, 0.5f);
        Vector2[] charactersSpacingCorners = DrawFrameFromBox(cachedTransform.position,
            new Vector2(Mathf.Clamp(charactersSpacing.x, 0f, maxCharactersFrame.x), Mathf.Clamp(charactersSpacing.y, 0f, maxCharactersFrame.y)));
    }

    private Vector2[] DrawFrameFromBox(Vector2 center, Vector2 size)
    {
        Vector2 bottomLeft = center + new Vector2(-size.x, -size.y) / 2;
        Vector2 topLeft = center + new Vector2(-size.x, size.y) / 2;
        Vector2 topRight = center + new Vector2(size.x, size.y) / 2;
        Vector2 bottomRight = center + new Vector2(size.x, -size.y) / 2;

        Handles.DrawLine(bottomLeft, topLeft);
        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);

        Vector2[] corners = { bottomLeft, topLeft, topRight, bottomRight };
        return corners;
    }

    private void DrawFrameFromCorners(Vector2[] corners)
    {
        Handles.DrawLine(corners[0], corners[1]);
        Handles.DrawLine(corners[1], corners[2]);
        Handles.DrawLine(corners[2], corners[3]);
        Handles.DrawLine(corners[3], corners[0]);
    }

    private void DrawFrameFromMinMaxCorners(Vector2 minCorner, Vector2 maxCorner)
    {
        Vector2 topLeftCorner = new Vector2(minCorner.x, maxCorner.y);
        Vector2 bottomRightCorner = new Vector2(maxCorner.x, minCorner.y);

        Handles.DrawLine(minCorner, topLeftCorner);
        Handles.DrawLine(topLeftCorner, maxCorner);
        Handles.DrawLine(maxCorner, bottomRightCorner);
        Handles.DrawLine(bottomRightCorner, minCorner);
    }
}
#endif
