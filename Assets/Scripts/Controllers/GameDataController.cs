using UnityEngine;
using System.Collections;

public class GameDataController : DataController {

	void Start () {
		
	}
	
	void Update () {

	}

	public void StartLevel(string otherPlayerName){
		player.Init();
		
		otherPlayer.Init(otherPlayerName);
		InitSession(PlayerPrefs.GetInt("last_game_with_" + otherPlayer.fullName));
	}
}
