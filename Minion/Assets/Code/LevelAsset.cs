using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Assets/New Level", order = 1)]
public class LevelAsset : ScriptableObject {
	public List<TileData> tiles = new List<TileData>();

	
}

[Serializable]
public class TileData {
	public Vector3 pos;
	public List<TileObject> tileObjects = new List<TileObject>();

	public TileData(Vector3 pos) {
		this.pos = pos;
	}
}

[Serializable]
public class TileObject {
	
}
