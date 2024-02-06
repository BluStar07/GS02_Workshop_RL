#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public partial class CharacterController : InputListener
{
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        // Cache
        Quaternion quatIdentity = Quaternion.identity;

        // Gizmos
        Gizmos.color = Handles.color = Color.cyan;

        // Wall detection
        /*float xDetectionOffset = 0.1f;
        Vector3 defaultPos = transform.position + new Vector3(-1 * xDetectionOffset, capsuleCollider.center.y, 0);
        float capsuleYOffsetFromCenter = (capsuleCollider.height - (capsuleCollider.radius * 2f)) / 2f;
        Vector3 bottomSPhere = defaultPos + new Vector3(0, -capsuleYOffsetFromCenter + feetDetectionOffset, 0);
        Vector3 topSphere = defaultPos + new Vector3(0, capsuleYOffsetFromCenter, 0);

        Handles.SphereHandleCap(-1, defaultPos, quatIdentity, 0.2f, EventType.Repaint);
        Handles.SphereHandleCap(-1, bottomSPhere, quatIdentity, capsuleCollider.radius * 2, EventType.Repaint);
        Handles.SphereHandleCap(-1, topSphere, quatIdentity, capsuleCollider.radius * 2, EventType.Repaint);*/

        // Motion vector
        Gizmos.color = Handles.color = Color.black;
        Handles.DrawLine(transform.position, transform.position + (Vector3)_movingVector * 2);

        Gizmos.color = Handles.color = Color.red;
        float targetSpeed = isMoving ? Mathf.Abs(leftStickAxisLerped.x) : 0f;
        Vector2 targetVector = _movingVector * targetSpeed;
        Handles.SphereHandleCap(-1, transform.position + (Vector3)targetVector * 2, quatIdentity, 0.2f, EventType.Repaint);
    }
}
#endif
