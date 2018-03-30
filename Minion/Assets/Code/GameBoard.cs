using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
	public LevelDatabase LevelDatabase;
	public GameObject RootTileObject;
	public HeroScript Hero;
	public GameObject TilePrefab;
	public GameObject StandPrefab;
	public GameObject GoalCameraPrefab;
	public RenderTexture GoalCameraTexture;
	private const int GRID_SIZE = 5;
	public TileInfo[,] grid = new TileInfo[GRID_SIZE, GRID_SIZE];
	public Dictionary<Vector3,TileInfo> gridDictionary = new Dictionary<Vector3,TileInfo>();
	
	private GameObject cameraGoal;

	Vector3 tileOffset = new Vector3 (-4,0,-4);
	
	void Start () {
		var level = LevelDatabase.levels[0];
		
		foreach (var tile in level.tiles) {
			var gameObjectTile = GameObject.Instantiate(Resources.Load<GameObject>("Tile"));
			gameObjectTile.transform.parent = RootTileObject.transform;
			gameObjectTile.transform.localEulerAngles = new Vector3 (0,0,0);
			gameObjectTile.transform.localPosition = new Vector3 (tile.pos.x*2,0.5f,tile.pos.z*2);
			var tileInfo = new TileInfo();
			gridDictionary.Add(tile.pos, tileInfo);
			foreach (var piece in tile.pieces) {
				GameObject tilePiece = null;
				if (piece.pieceType == PieceType.Hero) {
					Hero.transform.parent = gameObjectTile.transform;
					Hero.transform.localEulerAngles = new Vector3 (0,0,0);
					Hero.transform.localPosition = new Vector3 (0,0,0);
					
					Hero.transform.parent = RootTileObject.transform;
					Hero.transform.localPosition = new Vector3 (Hero.transform.localPosition.x,0.6f,Hero.transform.localPosition.z);
					continue;
				}
				if (piece.pieceType == PieceType.GoalPos) {
					tilePiece = GameObject.Instantiate (GoalCameraPrefab);
					tilePiece.transform.parent = gameObjectTile.transform;
					tilePiece.transform.localPosition = new Vector3 (0,0,0);
					cameraGoal = tilePiece;
					
					tilePiece.transform.parent = RootTileObject.transform;
					continue;
				}
				if (piece.pieceType == PieceType.Cube) {
					tilePiece = GameObject.Instantiate(Resources.Load<GameObject>("Stand"));	
				}

				if (tilePiece != null) {
					tilePiece.transform.parent = gameObjectTile.transform;
					tilePiece.transform.localEulerAngles = new Vector3 (0,0,0);
					tilePiece.transform.localPosition = new Vector3 (0,0,0);
					tileInfo.objects.Add (tilePiece);
				}
			}
		}
		cameraGoal.GetComponent<CameraPivot> ().SetTargetTexture (GoalCameraTexture);
		cameraGoal.transform.localEulerAngles = new Vector3 (0,270,0);
		cameraGoal.transform.localPosition = new Vector3 (cameraGoal.transform.localPosition.x,0.6f,cameraGoal.transform.localPosition.z);
		SetLayerInFront (cameraGoal.transform.localPosition, cameraGoal.transform.localEulerAngles.y);
	}

	int frameCounter = 0;
	void Update() {
		frameCounter++;
		if (frameCounter > 5) {
			cameraGoal.SetActive (false);
		}
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

		//var gridPos = pos-tileOffset;
		int gridPosX = Mathf.RoundToInt(pos.x/2);
		int gridPosZ = Mathf.RoundToInt(pos.z/2);

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
			foreach (var keyValuePair in gridDictionary) {
				if ((int)keyValuePair.Key.x == inputX-index) {
					resultTileInfos.Add(keyValuePair.Value);
				}
			}
		}
		if (dir == Direction.Right) {
			foreach (var keyValuePair in gridDictionary) {
				if ((int)keyValuePair.Key.z == inputZ+index) {
					resultTileInfos.Add(keyValuePair.Value);
				}
			}
		}
		if (dir == Direction.Down) {
			foreach (var keyValuePair in gridDictionary) {
				if ((int)keyValuePair.Key.x == inputX+index) {
					resultTileInfos.Add(keyValuePair.Value);
				}
			}
		}
		if (dir == Direction.Left) {
			foreach (var keyValuePair in gridDictionary) {
				if ((int)keyValuePair.Key.z == inputZ-index) {
					resultTileInfos.Add(keyValuePair.Value);
				}
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
