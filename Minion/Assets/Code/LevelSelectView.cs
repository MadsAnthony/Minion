using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectView : UIView {
	public GameObject levelButton;
	protected override void OnStart () {
		var rowLength = 5;
		int i = 0;
		Director.Instance.WorldIndex = 3;
		foreach(var level in Director.LevelDatabase.levels) {
			var levelButtonGo = Instantiate (levelButton);
			levelButtonGo.transform.parent = transform;
			levelButtonGo.transform.localScale = new Vector3 (1,1,1);
			levelButtonGo.transform.localPosition = new Vector3 (-200+i%rowLength*100,400-(i/rowLength)*100,0);

			int capturedIndex = i;
			levelButtonGo.GetComponent<Button> ().onClick.AddListener(() => { UIUtils.GotoLevelScene(capturedIndex);});
			/*
			if (Director.SaveData.GetLevelSaveDataEntry (i.ToString()) != null) {
				levelButtonGo.GetComponent<Image> ().color = new Color (118f/255,234f/255,62f/255,1);
			} else {
				levelButtonGo.GetComponent<Image> ().color = new Color (255f/255,84f/255,84f/255,1);;
			}
			*/
			levelButtonGo.GetComponentInChildren<Text> ().text = (i+1).ToString();
			i++;
		}
	}
}
