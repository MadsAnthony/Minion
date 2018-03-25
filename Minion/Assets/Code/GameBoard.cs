using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
	public GameObject RootTileObject;
	public GameObject TilePrefab;
	public GameObject StandPrefab;
	private const int GRID_SIZE = 5;
	public TileInfo[,] grid = new TileInfo[GRID_SIZE, GRID_SIZE];

	Vector3 tileOffset = new Vector3 (-4,0,-4);
	void Start () {
		SetupTiles ();


		AssignObjectToGrid (GameObject.Instantiate (StandPrefab),2,2);
		AssignObjectToGrid (GameObject.Instantiate (StandPrefab),3,3);
		AssignObjectToGrid (GameObject.Instantiate (StandPrefab),1,3);
		AssignObjectToGrid (GameObject.Instantiate (StandPrefab),1,4);
	}

	public void SetLayerInFront(Vector3 pos, float angle) {
		angle = Mathf.RoundToInt (angle + 360 % 360);
		Direction dir =  Direction.Up;
		if (angle == 0) {
			dir = Direction.Up;
		}
		if (angle == 90) {
			dir = Direction.Right;
		}
		if (angle == 180) {
			dir = Direction.Down;
		}
		if (angle == 270) {
			dir = Direction.Left;
		}

		var gridPos = pos-tileOffset;
		int gridPosX = Mathf.RoundToInt(gridPos.x/2);
		int gridPosZ = Mathf.RoundToInt(gridPos.z/2);

		var tileInfos0 = GetLocalRow (dir, gridPosX, gridPosZ, 1);
		foreach (var tileInfo in tileInfos0) {
			foreach (var gameObject in tileInfo.objects) {
				SetLayerRecursively (gameObject, "CameraLayer0");
			}
		}

		var tileInfos1 = GetLocalRow (dir, gridPosX, gridPosZ, 2);
		foreach (var tileInfo in tileInfos1) {
			foreach (var gameObject in tileInfo.objects) {
				SetLayerRecursively (gameObject, "CameraLayer1");
			}
		}

		var tileInfos2 = GetLocalRow (dir, gridPosX, gridPosZ, 3);
		foreach (var tileInfo in tileInfos2) {
			foreach (var gameObject in tileInfo.objects) {
				SetLayerRecursively (gameObject, "CameraLayer2");
			}
		}
		var tileInfos3 = GetLocalRow (dir, gridPosX, gridPosZ, 4);
		foreach (var tileInfo in tileInfos3) {
			foreach (var gameObject in tileInfo.objects) {
				SetLayerRecursively (gameObject, "CameraLayer2");
			}
		}
	}

	List<TileInfo> GetLocalRow(Direction dir, int inputX, int inputZ, int index) {
		var resultTileInfos = new List<TileInfo> ();
		if (dir == Direction.Up) {
			for (int z = 0; z<GRID_SIZE; z++) {
				resultTileInfos.Add (grid [Mathf.Clamp(inputX-index,0,GRID_SIZE-1), z]);
			}
		}
		if (dir == Direction.Right) {
			for (int x = 0; x<GRID_SIZE; x++) {
				resultTileInfos.Add (grid [x, Mathf.Clamp(inputZ+index,0,GRID_SIZE-1)]);
			}
		}
		if (dir == Direction.Down) {
			for (int z = 0; z<GRID_SIZE; z++) {
				resultTileInfos.Add (grid [Mathf.Clamp(inputX+index,0,GRID_SIZE-1), z]);
			}
		}
		if (dir == Direction.Left) {
			for (int z = 0;z<GRID_SIZE; z++) {
				resultTileInfos.Add (grid [z, Mathf.Clamp(inputZ-index,0,GRID_SIZE-1)]);
			}
		}
		return resultTileInfos;
	}

	void AssignObjectToGrid(GameObject newObject, int x, int z) {
		newObject.transform.parent = RootTileObject.transform;
		newObject.transform.localEulerAngles = new Vector3 (0,0,0);
		newObject.transform.localPosition = tileOffset + new Vector3 (x*2,0,z*2);
		grid [x, z].objects.Add (newObject);
	}

	void SetupTiles() {
		for (int x = 0; x<GRID_SIZE; x++) {
			for (int z = 0; z<GRID_SIZE; z++) {
				grid [x,z] = new TileInfo ();
				var tile = GameObject.Instantiate (TilePrefab);
				tile.transform.parent = RootTileObject.transform;
				tile.transform.localEulerAngles = new Vector3 (0,0,0);
				tile.transform.localPosition = tileOffset + new Vector3 (x*2,0.5f,z*2);
			}
		}
	}

	public void SetLayerRecursively(GameObject gameObject, string layerName) {
		var layerMask =LayerMask.NameToLayer(layerName);
		gameObject.layer = layerMask;
		foreach (Transform child in gameObject.transform) {
			SetLayerRecursively(child.gameObject, layerName);
		}
	}

	public class TileInfo {
		public List<GameObject> objects = new List<GameObject>();
	}

	public enum Direction { Up, Right, Down, Left}
}
