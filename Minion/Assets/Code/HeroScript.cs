using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroScript : MonoBehaviour {
	public GameObject Tile;
	public GameObject RootTileObject;
	public Animator Animator;
	public GameObject Cursor;
	public GameObject Picture;

	public Camera HeroCamera0;
	public Camera HeroCamera1;
	public Camera HeroCamera2;

	public Camera Camera;

	public GameBoard gameBoard;

	private Vector3 targetPos;
	private Coroutine moveCoroutine;
	private Coroutine rotateCoroutine;
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 60;

		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));

		targetPos = transform.position;
		Picture.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.R)) {
			Scene scene = SceneManager.GetActiveScene ();
			SceneManager.LoadScene (scene.name);
		}

		if (Input.GetMouseButton(0)) {
			RaycastHit hit;
			Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {
				if (Input.GetMouseButtonDown(0 )&& !isMoving && hit.collider.gameObject.name == "Hero" && !isRotating) {
					if (rotateCoroutine != null) StopCoroutine (rotateCoroutine);
					rotateCoroutine = StartCoroutine (Rotate ());
					return;
				}

				// If rotating stop everyting else.
				if (isRotating) {
					return;
				}

				targetPos = new Vector3 (hit.point.x, transform.position.y, hit.point.z);

				var targetPosLocal = RootTileObject.transform.InverseTransformPoint (targetPos);
				targetPosLocal = new Vector3 (Mathf.Round(targetPosLocal.x/2)*2, targetPosLocal.y, Mathf.Round(targetPosLocal.z/2)*2);
				Cursor.transform.localPosition = targetPosLocal;

				targetPos = RootTileObject.transform.TransformPoint (targetPosLocal);

				if (moveCoroutine != null) StopCoroutine (moveCoroutine);
				moveCoroutine = StartCoroutine (Move ());
			}
		}
	}

	bool isRotating;
	IEnumerator Rotate() {
		isRotating = true;
		Picture.SetActive (false);

		var startMousePos = Input.mousePosition;
		float t = 0;
		int lastRotation = 0;
		Vector3 startRotation = Vector3.zero;
		Vector3 startHeroLocalPos = transform.localPosition;
		Vector3 mouseLocalPositionOnGround = Vector3.zero;
		Coroutine rotateCoroutine = null;
		while (true) {
			if (Input.GetMouseButtonUp (0)) {
				break;
			}

			// Get local mouse position on ground. 
			RaycastHit hit;
			Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
			var layerMask = LayerMask.GetMask("GroundLayer");
			if (Physics.Raycast (ray, out hit,50,layerMask)) {
				var mousePositionOnGround = new Vector3 (hit.point.x, transform.position.y, hit.point.z);
				mouseLocalPositionOnGround = RootTileObject.transform.InverseTransformPoint (mousePositionOnGround);
			}

			// Calculate rotation and clamp it into four rotations (0,90,180,270).
			var vector = startHeroLocalPos-mouseLocalPositionOnGround;
			int fourRotations = (int)Mathf.Round (((Mathf.Atan2 (vector.x, vector.z)) / Mathf.PI + 1) * 2);
			var rotation = ((fourRotations / 2f) - 1) * Mathf.PI;

			if (fourRotations != lastRotation) {
				if (rotateCoroutine != null) StopCoroutine (rotateCoroutine);
				rotateCoroutine = StartCoroutine (RotateTo (transform, rotation * Mathf.Rad2Deg - 90, 0.2f));
			}

			lastRotation = fourRotations;
			yield return null;
		}

		// Update Camera
		HeroCamera0.enabled = false;
		HeroCamera0.enabled = true;
		HeroCamera1.enabled = false;
		HeroCamera1.enabled = true;
		HeroCamera2.enabled = false;
		HeroCamera2.enabled = true;

		gameBoard.SetLayerInFront (transform.localPosition, transform.localEulerAngles.y);
		Picture.SetActive (true);

		isRotating = false;
	}

	IEnumerator RotateTo(Transform transform, float toAngle, float time) {
		float t = 0;
		var startAngle = transform.localEulerAngles;
		var angleA = (startAngle.y + 360) % 360;
		var angleB = (toAngle + 360) % 360;

		var dist = angleB-angleA;
		if (Mathf.Abs (dist)>180) {
			dist = -Mathf.Sign (dist) * (360 - Mathf.Abs (dist));
		}

		while (true) {
			t += 1/time*Time.deltaTime;

			transform.localEulerAngles = startAngle + Mathf.Clamp01(t) * new Vector3 (0, dist, 0);

			if (t >= 1) {
				break;
			}
			yield return null;
		}
	}

	bool isMoving;
	IEnumerator Move() {
		isMoving = true;
		Picture.SetActive (false);

		float t = 0;
		var startPos = transform.position;
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.run));

		var dirVector = targetPos-startPos;
		transform.localEulerAngles = new Vector3 (0,Mathf.Atan2(dirVector.x,dirVector.z)*Mathf.Rad2Deg+45,0);
		while (true) {
			t += 4*(1f/dirVector.magnitude)*Time.deltaTime;
			transform.position = Vector3.Lerp (startPos, targetPos, t);
			if (t>1) {
				break;
			}
			yield return null;
		}
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));
		isMoving = false;
	}

	enum HeroAnimationState {
		idle,
		run
	}
}
