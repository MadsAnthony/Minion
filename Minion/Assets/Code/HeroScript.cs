using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroScript : MonoBehaviour {

	public GameObject Tile;
	public GameObject RootTileObject;
	public Animator Animator;
	public GameObject Cursor;

	public Camera Camera;

	private Vector3 targetPos;
	private Coroutine moveCoroutine;
	// Use this for initialization
	void Start () {
		SetupTiles ();

		Application.targetFrameRate = 60;

		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));

		targetPos = transform.position;
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

		//Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));
		if (Input.GetMouseButton(0)) {
			//transform.position += new Vector3 (1,0,1)*Time.deltaTime*-3;


			RaycastHit hit;
			Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {
				Transform objectHit = hit.transform;

				targetPos = new Vector3 (hit.point.x, transform.position.y, hit.point.z);

				var targetPosLocal = RootTileObject.transform.InverseTransformPoint (targetPos);
				targetPosLocal = new Vector3 (Mathf.Round(targetPosLocal.x/2)*2, targetPosLocal.y, Mathf.Round(targetPosLocal.z/2)*2);
				Cursor.transform.localPosition = targetPosLocal;

				targetPos = RootTileObject.transform.TransformPoint (targetPosLocal);

				if (moveCoroutine != null) StopCoroutine (moveCoroutine);
				moveCoroutine = StartCoroutine (Move ());
				// Do something with the object that was hit by the raycast.
			}
		}
		//transform.position += (targetPos-transform.position)*Time.deltaTime;
	}

	IEnumerator Move() {
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
	}

	enum HeroAnimationState {
		idle,
		run
	}
}
