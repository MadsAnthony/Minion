using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroScript : MonoBehaviour {

	public Animator Animator;
	// Use this for initialization
	void Start () {
		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.R)) {
			Scene scene = SceneManager.GetActiveScene ();
			SceneManager.LoadScene (scene.name);
		}

		Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.idle));
		if (Input.GetMouseButton(0)) {
			transform.position += new Vector3 (1,0,1)*Time.deltaTime*-3;
			Animator.SetInteger ("stateIndex", (int)(HeroAnimationState.run));
		}
	}

	enum HeroAnimationState {
		idle,
		run
	}
}
