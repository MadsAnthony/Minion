using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Assets/New Level", order = 1)]
public class LevelAsset : ScriptableObject {
	public List<TileData> Tiles = new List<TileData>();

	
}

[Serializable]
public class TileData {
	public Vector3 Pos;
	public List<PieceData> Pieces = new List<PieceData>();

	public TileData(Vector3 pos) {
		this.Pos = pos;
	}
}

[Serializable]
public class PieceData {
	public PieceType PieceType;
	public Direction Direction;

	public PieceData(PieceType pieceType) {
		this.PieceType = pieceType;
	}
}

// ATTENTION: Always add new entries at the end and be careful when removing entries (as enums are serialized to integers).
public enum PieceType {
	Tile,
	Hero,
	GoalPos,
	Cube
};

public enum Direction { Up, Right, Down, Left}
