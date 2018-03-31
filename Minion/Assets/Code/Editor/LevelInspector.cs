using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;

[CustomEditor(typeof(LevelAsset))]
public class LevelInspector : Editor {
	private RenderTexture editorRenderTexture;
	private Vector2 windowOffset = new Vector2(20,100);
	private Vector2 windowSize = new Vector2(500,500);
	private Color windowBackgroundColor = new Color (1,1,1,1);
	private int windowGridSize = 100;

	private Vector2 startMousePos;
	private Vector2 mousePos;
	private Vector3 selectPos;
	private PieceType pieceType;
	private EditorMode editorMode;
	private TileData selectableTile = null;
	public override void OnInspectorGUI() {
		bool reconstruct = false;
		LevelAsset myTarget = (LevelAsset)target;

		var editorModeCached = editorMode;
		string[] editorModeOptions = {"Select", "Add"};
		editorMode = (EditorMode)EditorGUILayout.Popup ("Mode", (int)editorMode, editorModeOptions);

		if (editorModeCached != editorMode) {
			reconstruct = true;
		}

		if (editorMode == EditorMode.Add) {
			string[] pieceOptions = Enum.GetNames (typeof(PieceType));
			pieceType = (PieceType)EditorGUILayout.Popup ("Piece Type", (int)pieceType, pieceOptions);
		}

		if (cameraGameObject != null) {
			EditorGUI.DrawPreviewTexture (new Rect (0+windowOffset.x, 0+windowOffset.y, windowSize.x, windowSize.y), editorRenderTexture);
		}

		var tmpMousePos = Event.current.mousePosition;
		tmpMousePos -= windowOffset;
		tmpMousePos -= windowSize * 0.5f;
		var mousePosInGrid = new Vector3(Mathf.RoundToInt(tmpMousePos.x / windowGridSize), 0, -Mathf.RoundToInt(tmpMousePos.y / windowGridSize));
		if (Event.current.type == EventType.MouseDown && IsPositionWithinWindow(Event.current.mousePosition)) {
			if (editorMode == EditorMode.Select) {
				selectPos = mousePosInGrid;
				selectableTile = myTarget.Tiles.Find (x => {return x.Pos == mousePosInGrid;});
					
				reconstruct = true;
			} else if (editorMode == EditorMode.Add) {
				if (pieceType == PieceType.Tile) {
					if (Event.current.button == 0) {
						if (!myTarget.Tiles.Exists(x => { return x.Pos == mousePosInGrid; })) {
							myTarget.Tiles.Add(new TileData(mousePosInGrid));
							reconstruct = true;
						}
					}

					if (Event.current.button == 1) {
						var posibleTile = myTarget.Tiles.Find(x => { return x.Pos == mousePosInGrid; });
						if (posibleTile != null) {
							myTarget.Tiles.Remove(posibleTile);
							reconstruct = true;
						}
					}
				} else {
					var posibleTile = myTarget.Tiles.Find(x => { return x.Pos == mousePosInGrid; });
					if (Event.current.button == 0) {
						if (posibleTile != null && posibleTile.Pieces.Count == 0) {
							posibleTile.Pieces.Add(new PieceData(pieceType));
							reconstruct = true;
						}
					}
					if (Event.current.button == 1) {
						if (posibleTile != null && posibleTile.Pieces.Count != 0){
							posibleTile.Pieces = new List<PieceData>();
							reconstruct = true;
						}
					}
				}
			}	

		}

		if (Event.current.button == 2) {
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

		if (editorMode == EditorMode.Select && selectableTile != null) {
			EditorGUILayout.BeginVertical();
			GUILayout.Space (windowOffset.y+windowSize.y);


			ReorderableListGUI.Title("Selection");
			ReorderableListGUI.ListField<PieceData>(selectableTile.Pieces, SelectionOfPieceDrawer);
			EditorGUILayout.EndVertical ();
		}

		EditorUtility.SetDirty (myTarget);
	}

	bool IsPositionWithinWindow(Vector2 pos) {
		return 	pos.x > windowOffset.x && pos.x < windowOffset.x+windowSize.x &&
				pos.y > windowOffset.y && pos.y < windowOffset.y+windowSize.y;
	}
	
	private void OnEnable() {
		if (Application.isPlaying) {
			DestroyLevel ();
		} else {
			ConstructLevel ();
		}
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
		cameraGameObject.hideFlags = HideFlags.HideAndDontSave;
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = windowBackgroundColor;
		editorRenderTexture = Resources.Load ("EditorRenderTexture") as RenderTexture;
		camera.targetTexture = editorRenderTexture;

		if (editorMode == EditorMode.Select) {
			var editorSelect = GameObject.Instantiate (Resources.Load<GameObject> ("EditorSelect"));
			AssignObjectToGrid (editorSelect, (int)selectPos.x, (int)selectPos.z);
			editorSelect.transform.localPosition = new Vector3 (editorSelect.transform.localPosition.x, 2, editorSelect.transform.localPosition.z);
		}

		foreach (var tile in myTarget.Tiles) {
			var gameObjectTile = GameObject.Instantiate(Resources.Load<GameObject>("Tile"));
			foreach (var piece in tile.Pieces) {
				GameObject tilePiece = null;
				if (piece.PieceType == PieceType.Hero) {
					tilePiece = GameObject.Instantiate(Resources.Load<GameObject>("Hero"));
				}
				if (piece.PieceType == PieceType.GoalPos) {
					tilePiece = GameObject.Instantiate(Resources.Load<GameObject>("GoalPos"));	
				}
				if (piece.PieceType == PieceType.Cube) {
					tilePiece = GameObject.Instantiate(Resources.Load<GameObject>("Stand"));	
				}

				if (tilePiece != null) {
					tilePiece.transform.parent = gameObjectTile.transform;
					tilePiece.transform.localEulerAngles = new Vector3 (0,0,0);
					tilePiece.transform.localPosition = new Vector3 (0,-0.5f,0);
				}
			}
			AssignObjectToGrid(gameObjectTile,(int)tile.Pos.x,(int)tile.Pos.z);
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
	}
	
	private void DestroyLevel() {
		GameObject.DestroyImmediate (rootContainer);
		GameObject.DestroyImmediate (cameraGameObject);
	}

	public enum EditorMode {
		Select,
		Add
	};

	PieceData SelectionOfPieceDrawer(Rect rect, PieceData value) {
		var r = new Rect (rect);
		if (value != null) {
			r.width = 100;
			value.PieceType = (PieceType)EditorGUI.EnumPopup (r, value.PieceType);

			r.x += 100;
			value.Direction = (Direction)EditorGUI.EnumPopup (r, value.Direction);
		}
		return value;
	}
}
