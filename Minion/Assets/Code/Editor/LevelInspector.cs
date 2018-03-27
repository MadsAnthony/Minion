using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(LevelAsset))]
public class LevelInspector : Editor {
	private RenderTexture editorRenderTexture;
	private Vector2 windowOffset = new Vector2(20,100);
	private Vector2 windowSize = new Vector2(500,500);
	private Color windowBackgroundColor = new Color (1,1,1,1);

	Vector2 startMousePos;
	Vector2 mousePos;
	public override void OnInspectorGUI()
	{
		LevelAsset myTarget = (LevelAsset)target;
		if (cameraGameObject != null) {
			EditorGUI.DrawPreviewTexture (new Rect (0+windowOffset.x, 0+windowOffset.y, windowSize.x, windowSize.y), editorRenderTexture);
		}
		//EditorGUI.DrawRect (new Rect (10, 10, 60, 60), Color.blue);
		//if (Event.current.type == EventType.MouseDrag) {

		if (Event.current.button == 2) {
			//mousePos = Event.current.mousePosition;
			if (Event.current.type == EventType.MouseDown) {
				mousePos = Event.current.mousePosition;
			}
			if (Event.current.type == EventType.MouseDrag) {
				var mouseDir = (mousePos - Event.current.mousePosition) * 0.01f;
				cameraGameObject.transform.position += new Vector3 (mouseDir.x, -mouseDir.y, 0);
				mousePos = Event.current.mousePosition;
			}
			//cameraGameObject.transform.position += new Vector3 (0.2f, 0, 0);
		}
		//}
		EditorUtility.SetDirty (myTarget);
	}

	private void OnEnable() {
		ConstructLevel ();
	}

	private void OnDisable() {
		DestroyLevel ();
	}

	private GameObject cameraGameObject;
	private void ConstructLevel() {
		cameraGameObject = new GameObject();
		cameraGameObject.transform.position = new Vector3 (0,0,-12);
		cameraGameObject.hideFlags = HideFlags.HideAndDontSave;
		var camera = cameraGameObject.AddComponent<Camera> ();
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = windowBackgroundColor;
		editorRenderTexture = Resources.Load ("EditorRenderTexture") as RenderTexture;
		camera.targetTexture = editorRenderTexture;
	}

	private void DestroyLevel() {
		GameObject.DestroyImmediate (cameraGameObject);
	}
}
