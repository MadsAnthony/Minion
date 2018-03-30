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
	public List<Piece> pieces = new List<Piece>();

	public TileData(Vector3 pos) {
		this.pos = pos;
	}
}

[Serializable]
public class Piece {
	public PieceType pieceType;
	
	public Piece(PieceType pieceType) {
		this.pieceType = pieceType;
	}
}

// ATTENTION: Always add new entries at the end and be careful when removing entries (as enums are serialized to integers).
public enum PieceType {
	Tile,
	Hero,
	GoalPos,
	Cube
};
