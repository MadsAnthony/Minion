using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour {
	void Start () {
		var initDirector = Director.Instance;
		SceneManager.LoadScene ("LevelScene");
	}
}
