using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LevelAsset))]
public class LevelInspector : Editor {
	private RenderTexture editorRenderTexture;
	private Vector2 windowOffset = new Vector2(20,100);
	private Vector2 windowSize = new Vector2(500,500);
	private Color windowBackgroundColor = new Color (1,1,1,1);
	private int windowGridSize = 100;

	Vector2 startMousePos;
	Vector2 mousePos;
	public override void OnInspectorGUI()
	{
		bool reconstruct = false;
		LevelAsset myTarget = (LevelAsset)target;
		if (cameraGameObject != null) {
			EditorGUI.DrawPreviewTexture (new Rect (0+windowOffset.x, 0+windowOffset.y, windowSize.x, windowSize.y), editorRenderTexture);
		}

		var tmpMousePos = Event.current.mousePosition;
		tmpMousePos -= windowOffset;
		tmpMousePos -= windowSize * 0.5f;
		var mousePosInGrid = new Vector3(Mathf.RoundToInt(tmpMousePos.x / windowGridSize), 0, -Mathf.RoundToInt(tmpMousePos.y / windowGridSize));
		if (Event.current.type == EventType.MouseDown) {
			
			if (Event.current.button == 0) {
				if (!myTarget.tiles.Exists(x => { return x.pos == mousePosInGrid; })) {
					myTarget.tiles.Add(new TileData(mousePosInGrid));
					reconstruct = true;
				}
			}
			
			if (Event.current.button == 1){
				var posibleTile = myTarget.tiles.Find(x => { return x.pos == mousePosInGrid; });
				if (posibleTile != null) {
					myTarget.tiles.Remove(posibleTile);
					reconstruct = true;
				}
			}
		}

		if (Event.current.button == 2) {
			//mousePos = Event.current.mousePosition;
			if (Event.current.type == EventType.MouseDown) {
				mousePos = Event.current.mousePosition;
			}
			if (Event.current.type == EventType.MouseDrag) {
				var mouseDir = (mousePos - Event.current.mousePosition) * 0.01f;
				cameraGameObject.transform.position += new Vector3 (mouseDir.x, 0, -mouseDir.y);
				mousePos = Event.current.mousePosition;
			}
		}

		if (reconstruct) {
			DestroyLevel ();
			ConstructLevel();
		}
		EditorUtility.SetDirty (myTarget);
	}

	private bool TilesContainsPos(Vector3 pos) {
		LevelAsset myTarget = (LevelAsset)target;
		foreach (var tile in myTarget.tiles) {
			if (tile.pos == pos) {
				return true;
			}
		}

		return false;
	}
	
	private void OnEnable() {
		ConstructLevel ();
	}

	private void OnDisable() {
		DestroyLevel ();
	}

	private GameObject cameraGameObject;
	private GameObject rootContainer;
	private void ConstructLevel() {
		LevelAsset myTarget = (LevelAsset)target;
		rootContainer = new GameObject();
		rootContainer.transform.position = new Vector3 (0,0,-100);
		
		cameraGameObject = new GameObject();
		cameraGameObject.transform.position = new Vector3 (0,4,-100);
		cameraGameObject.transform.eulerAngles = new Vector3(90,0,0);
		var camera = cameraGameObject.AddComponent<Camera> ();
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = windowBackgroundColor;
		editorRenderTexture = Resources.Load ("EditorRenderTexture") as RenderTexture;
		camera.targetTexture = editorRenderTexture;

		foreach (var tile in myTarget.tiles) {
			var gameObjectTile = GameObject.Instantiate(Resources.Load<GameObject>("Tile"));
			AssignObjectToGrid(gameObjectTile,(int)tile.pos.x,(int)tile.pos.z);
		}

		SetHideFlagsRecursively(rootContainer);
	}

	public void SetHideFlagsRecursively(GameObject gameObject) {
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		foreach (Transform child in gameObject.transform) {
			SetHideFlagsRecursively(child.gameObject);
		}
	}
	
	void AssignObjectToGrid(GameObject newObject, int x, int z) {
		newObject.transform.parent = rootContainer.transform;
		newObject.transform.localEulerAngles = new Vector3 (0,0,0);
		newObject.transform.localPosition = new Vector3 (x*2,0,z*2);
		//grid [x, z].objects.Add (newObject);
	}
	
	private void DestroyLevel() {
		GameObject.DestroyImmediate (rootContainer);
		GameObject.DestroyImmediate (cameraGameObject);
	}
}
