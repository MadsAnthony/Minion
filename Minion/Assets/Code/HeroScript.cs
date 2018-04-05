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

	public RenderTexture CameraRenderTexture;
	public RenderTexture GoalRenderTexture;
	public Material MatchMaterial;
	public GameObject MatchPicture;

	public Camera Camera;

	public GameBoard gameBoard;

	private Vector3 targetPos;
	private Coroutine moveCoroutine;
	private Coroutine rotateCoroutine;

	// Use this for initialization
	void Start () {
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));

		targetPos = transform.position;
		Picture.SetActive (false);
		MatchPicture.SetActive (false);
		Director.GameEventManager.OnGameEvent += OnGameEvent;
	}

	void OnDestroy() {
		Director.GameEventManager.OnGameEvent -= OnGameEvent;
	}

	void OnGameEvent(GameEvent e) {
		switch (e.type) {
			case GameEventType.LevelCompleted:
				Director.TransitionManager.PlayTransition(()=> {
				Director.Instance.LevelIndex = Mathf.Clamp(Director.Instance.LevelIndex+1,0,Director.LevelDatabase.levels.Count-1);
					Scene scene = SceneManager.GetActiveScene ();
					SceneManager.LoadScene(scene.name);
				},
				0.2f,Director.TransitionManager.FadeToBlack(),Director.TransitionManager.FadeOut());
				break;
			default:
				break;
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

					rotateCoroutine = StartCoroutine (GameBoard.RotateTo (transform, Mathf.RoundToInt(transform.localEulerAngles.y/90)*90 + 90, 0.2f, (bool isDone) => {this.isRotating = !isDone;}));
					return;
				}

				if (hit.collider.gameObject.name == "Hero") {
					return;
				}
			}

			RaycastHit hit2;
			Ray ray2 = Camera.ScreenPointToRay(Input.mousePosition);
			var layerMask = LayerMask.GetMask("TileLayer");
			if (Physics.Raycast (ray2, out hit,50,layerMask)) {
				if (!isMoving) {
				targetPos = new Vector3 (hit.point.x, transform.position.y, hit.point.z);

				var targetPosLocal = RootTileObject.transform.InverseTransformPoint (targetPos);
				targetPosLocal = new Vector3 (Mathf.Round(targetPosLocal.x/2)*2, targetPosLocal.y, Mathf.Round(targetPosLocal.z/2)*2);
				Cursor.transform.localPosition = targetPosLocal;

				targetPos = RootTileObject.transform.TransformPoint (targetPosLocal);


					moveCoroutine = StartCoroutine (Move (targetPosLocal));
				}
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
				rotateCoroutine = StartCoroutine (GameBoard.RotateTo (transform, rotation * Mathf.Rad2Deg - 90, 0.2f));
			}

			lastRotation = fourRotations;
			yield return null;
		}



		isRotating = false;
	}

	private Coroutine takePictureCoroutine;
	public void TakePicture() {
		if (takePictureCoroutine != null) StopCoroutine (takePictureCoroutine);
		takePictureCoroutine = StartCoroutine (TakePictureCr ());
	}

	public IEnumerator TakePictureCr() {
		// Update Camera
		HeroCamera0.enabled = false;
		HeroCamera0.enabled = true;
		HeroCamera1.enabled = false;
		HeroCamera1.enabled = true;
		HeroCamera2.enabled = false;
		HeroCamera2.enabled = true;

		gameBoard.SetLayerInFront (transform.localPosition, transform.localEulerAngles.y);
		Picture.SetActive (true);

		yield return null;
		MatchPicture.SetActive (Director.Instance.ShowMatchPrecision);

		var precision = CheckPictureIsGoal ();
		if (precision>0.994f) {
			Director.GameEventManager.Emit (GameEventType.LevelCompleted);
		}
	}

	private float CheckPictureIsGoal() {
		int width = 256;
		int height = 256;
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
		RenderTexture.active = CameraRenderTexture;
		texture.ReadPixels(new Rect(0,0,width,height), 0, 0);
		var currentTexturePixels = texture.GetPixels ();


		RenderTexture.active = null;

		Texture2D textureGoal = new Texture2D(width, height, TextureFormat.RGBA32, false);
		RenderTexture.active = GoalRenderTexture;
		textureGoal.ReadPixels(new Rect(0,0,width,height), 0, 0);
		var goalTexturePixels = textureGoal.GetPixels ();

		RenderTexture.active = null;

		Color[] matchPixels = new Color[width*height];
		int sum = 0;
		for (int i = 0; i<width*height; i++) {
			var pixel = currentTexturePixels[i];

			matchPixels [i] = new Color (1,1,1,0f);
			if (currentTexturePixels [i] != new Color (190/255f,190/255f,190/255f,0) && goalTexturePixels [i] == new Color (190/255f,190/255f,190/255f,0)) {
				matchPixels [i] = new Color (1, 0, 0, 0.5f);
			}
			if (pixel == goalTexturePixels [i]) {
				sum++;
				if (goalTexturePixels [i] != new Color (190/255f,190/255f,190/255f,0)) {
					matchPixels [i] = new Color (0, 1, 0, 0.5f);
				}
			}
		}
		var precision = sum / (float)(width * height);


		Texture2D textureMatch = new Texture2D(width, height, TextureFormat.RGBA32, false);
		textureMatch.SetPixels (matchPixels);
		MatchMaterial.mainTexture = textureMatch;
		textureMatch.Apply ();

		return precision;
	}

	bool isMoving;
	IEnumerator Move(Vector3 newPos) {
		isMoving = true;
		Picture.SetActive (false);
		MatchPicture.SetActive (false);

		float t = 0;
		var startPos = transform.localPosition;
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.run));

		var dirVector = newPos-startPos;
		transform.localEulerAngles = new Vector3 (0,Mathf.Atan2(dirVector.x,dirVector.z)*Mathf.Rad2Deg+90,0);
		while (true) {
			t += 4*(1f/dirVector.magnitude)*Time.deltaTime;
			transform.localPosition = Vector3.Lerp (startPos, newPos, t);
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
