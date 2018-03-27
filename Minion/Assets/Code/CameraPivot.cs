using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour {
	public Camera[] Cameras;
	public Shader ReplacementShader;
	public void SetTargetTexture(RenderTexture texture) {
		foreach (var camera in Cameras) {
			camera.enabled = false;
			camera.enabled = true;
			camera.targetTexture = texture;
		}
	}

	void OnEnable() {
		foreach (var camera in Cameras) {
			camera.SetReplacementShader (ReplacementShader, "");
		}
	}
}
