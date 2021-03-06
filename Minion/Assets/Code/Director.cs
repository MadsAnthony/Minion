﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour  {
	private static Director instance;
	private static bool hasBeenDestroyed;

	[SerializeField] private LevelDatabase levelDatabase;
	[SerializeField] private SoundDatabase soundDatabase;

	private int levelIndex = 0;
	private int prevLevelIndex = -1;
	public int LevelIndex 
	{
		get {return levelIndex;}
		set 
		{
			prevLevelIndex = levelIndex;
			levelIndex = value;
		}
	}
	public int PrevLevelIndex {
		get { return prevLevelIndex;}
	}
	private int worldIndex = 1;
	public int WorldIndex {
		get {return worldIndex;}
		set {worldIndex = value;}
	}

	public bool ShowMatchPrecision { get; set;}

	private GameEventManager		gameEventManager;
	private UIManager 		 		uiManager;
	private TransitionManager		transitionManager;

	public static GameEventManager 		GameEventManager 	{get {return Instance.gameEventManager;}}
	public static LevelDatabase    		LevelDatabase 		{get {return Instance.levelDatabase;}}
	public static UIManager    	   		UIManager			{get {return Instance.uiManager;}}
	public static TransitionManager		TransitionManager 	{get {return Instance.transitionManager;}}
	public static SoundDatabase			Sounds 				{get {return Instance.soundDatabase;}}

	public static Director Instance
	{
		get
		{
			if (instance == null && !Director.hasBeenDestroyed)
			{
				var asset = (Director)Resources.Load ("Director", typeof(Director));
				instance = (Director)GameObject.Instantiate (asset);
				instance.Load();
			}
			return instance;
		}
	}

	void OnDestroy() {
		Director.hasBeenDestroyed = true;
	}

	void Load() {
		gameEventManager  = new GameEventManager();
		uiManager 		  = new UIManager();
		transitionManager = SetupTransitionManager();
	}

	void Start () {
		DontDestroyOnLoad (transform.gameObject);
		Application.targetFrameRate = 60;
		new DeveloperCheats ();
	}

	TransitionManager SetupTransitionManager() {
		var asset = (TransitionManager)Resources.Load ("TransitionManager", typeof(TransitionManager));
		return (TransitionManager)GameObject.Instantiate (asset);
	}

	public static void CameraShake() {
		GameObject.Find ("BaseCamera").GetComponentInParent<CameraManager>().CameraShake();
	}
}
