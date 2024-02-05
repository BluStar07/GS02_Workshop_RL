using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovingPlatform : MonoBehaviour {

	[Range(0f, 10f)]
	public float speed = 1f;
	public Vector2[] positions;
	[HideInInspector] public Vector2[] truePositions;
	[Range(0f, 5f)]
	public float stopDelay = 0f;
	public bool loop = false;

	private int positionIndex;
	private int positionOffset = 1;

	private Transform player;
	//private PlayerBehaviour playerBehaviour;
	private Coroutine moveCoroutine;

	private Vector3 previousPos;
	public Vector3 velocity;

	private Coroutine unparentCoroutine;

	private void Start()
	{
		// Empty positions list
		if (positions.Length == 0)
			return;

		CreateTruePositions();

		positionIndex = 0;
		GoToNextPosition();
	}

	private void Update()
	{
		velocity = (transform.position - previousPos) / Time.deltaTime;
		previousPos = transform.position;
	}

	public void CreateTruePositions ()
	{
		// Empty positions list
		if (positions.Length == 0)
			return;

		truePositions = new Vector2[positions.Length + 1];
		truePositions[0] = transform.position;
		for (int i = 1; i < truePositions.Length; i++)
		{
			truePositions[i] = positions[i - 1];
		}
	}

	private void GoToNextPosition ()
	{
		if (moveCoroutine != null)
			StopCoroutine(moveCoroutine);
		moveCoroutine = StartCoroutine(CoGoToNextPosition());
	}

	private IEnumerator CoGoToNextPosition ()
	{
		Vector3 previousPos = truePositions[positionIndex];

		if (positionIndex == positions.Length)
		{
			if (loop)
				positionIndex = -1;
			else
				positionOffset = -1;
		}

		if (positionIndex == 0)
			positionOffset = 1;

		positionIndex += positionOffset;

		float t = 0f;
		float delay = Vector3.Distance(previousPos, truePositions[positionIndex]) / speed;

		while (t < delay)
		{
			t += Time.deltaTime;

			transform.position = Vector3.Lerp(previousPos, truePositions[positionIndex], t / delay);

			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(stopDelay);

		GoToNextPosition();
	}

	private void OnCollisionEnter(Collision collision)
	{
		/*if (!GetComponent<Obstacle>())
		{
			//player = collision.transform;
			//player.SetParent(transform);

			if (unparentCoroutine != null)
				StopCoroutine(unparentCoroutine);

			playerBehaviour = PlayerBehaviour.Instance;
			playerBehaviour.SetParentPlatform(this);
		}*/
	}

	private void OnCollisionExit(Collision collision)
	{
		/*if (playerBehaviour)
			UnsetParenting();*/
	}

	private void UnsetParenting ()
	{
		if (unparentCoroutine != null)
			StopCoroutine(unparentCoroutine);
		unparentCoroutine = StartCoroutine(CoUnsetParenting());
	}

	private IEnumerator CoUnsetParenting ()
	{
		yield return new WaitForSeconds(0.1f);

		/*playerBehaviour.RemoveParentPlatform();
		playerBehaviour = null;*/
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!EditorApplication.isPlaying)
			CreateTruePositions();

		DrawPositions();	
	}

	private void DrawPositions()
	{
		Gizmos.color = Handles.color = Color.magenta;

		Handles.CircleHandleCap(-1, transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
		if (loop)
			Handles.DrawDottedLine(truePositions[truePositions.Length - 1], truePositions[0], 5f);

		for (int i = 0; i < truePositions.Length; i++)
		{
			Handles.SphereHandleCap(-1, truePositions[i], Quaternion.identity, 0.25f, EventType.Repaint);
			Handles.Label(truePositions[i], "Position " + i);

			if (i > 0)
				Handles.DrawDottedLine(truePositions[i - 1], truePositions[i], 5f);
		}
	}
#endif
}
