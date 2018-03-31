using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opencoding.CommandHandlerSystem;
using UnityEngine.SceneManagement;

public class DeveloperCheats {

	static DeveloperCheats() {
		CommandHandlers.RegisterCommandHandlers(typeof(DeveloperCheats));
	}

	[CommandHandler]
	private static void DeleteAllSaveData() {
		PlayerPrefs.DeleteAll ();
	}

	[CommandHandler]
	private static void GotoLevelSelectScene() {
		SceneManager.LoadScene ("LevelSelectScene");
	}

	[CommandHandler]
	private static void ToggleMatchPrecision() {
		Director.Instance.ShowMatchPrecision = !Director.Instance.ShowMatchPrecision;
	}

}
