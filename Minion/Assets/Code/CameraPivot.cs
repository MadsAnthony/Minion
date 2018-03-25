using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour {
	public Camera[] Cameras;
	public void SetTargetTexture(RenderTexture texture) {
		foreach (var camera in Cameras) {
			camera.enabled = false;
			camera.enabled = true;
			camera.targetTexture = texture;
		}
	}
}
