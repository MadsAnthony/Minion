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

	public Camera Camera;

	private Vector3 targetPos;
	private Coroutine moveCoroutine;
	private Coroutine rotateCoroutine;
	// Use this for initialization
	void Start () {
		SetupTiles ();

		Application.targetFrameRate = 60;

		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));

		targetPos = transform.position;
		Picture.SetActive (false);
	}

	void SetupTiles() {
		Vector3 tileOffset = new Vector3 (-4,0,-4);
		for (int x = 0; x<5; x++) {
			for (int z = 0; z<5; z++) {
				var tile = GameObject.Instantiate (Tile);
				tile.transform.parent = RootTileObject.transform;
				tile.transform.localEulerAngles = new Vector3 (0,0,0);
				tile.transform.localPosition = tileOffset + new Vector3 (x*2,0.5f,z*2);
			}
		}

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
		while (true) {
			if (Input.GetMouseButtonUp (0)) {
				break;
			}
			var vector = startMousePos-Input.mousePosition;
			int fourRotations = (int)Mathf.Round((Mathf.Atan2 (vector.x, vector.y)/Mathf.PI+1)*2);
			var rotation = ((fourRotations/2f)-1)*Mathf.PI;

			if (fourRotations != lastRotation) {
				t = 0;
				startRotation = transform.localEulerAngles;
			}
			t += Time.deltaTime*4;
			//transform.localEulerAngles = Vector3.Lerp (startRotation, new Vector3 (0,rotation*Mathf.Rad2Deg, 0), t);
			transform.localEulerAngles = new Vector3 (0,rotation*Mathf.Rad2Deg, 0);
			lastRotation = fourRotations;
			yield return null;
		}
		Picture.SetActive (true);
		isRotating = false;
	}

	bool isMoving;
	IEnumerator Move() {
		isMoving = true;
		Picture.SetActive (false);

		float t = 0;
		var startPos = transform.position;
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.run));

		var dirVector = targetPos-startPos;
		transform.localEulerAngles = new Vector3 (0,Mathf.Atan2(dirVector.x,dirVector.z)*Mathf.Rad2Deg-45+180,0);
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
