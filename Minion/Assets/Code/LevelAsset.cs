using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CreateAssetMenu(fileName = "Level", menuName = "Assets/New Level", order = 1)]
public class LevelAsset : ScriptableObject {
	public List<Tile> tiles;

	public class Tile {
	}
}
