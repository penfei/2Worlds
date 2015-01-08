using UnityEngine;
using System.Collections;

public class DataController: MonoBehaviour {

	public UserData player = new UserData();
	public OtherPlayerData otherPlayer = new OtherPlayerData();
	public SessionData session = new SessionData();
	
	public class UserData {
		public string userName;
		public string userId;
		
		public void Init(){
			userId = PlayerPrefs.GetString("user_id");
			if (string.IsNullOrEmpty(userId))
			{
				userId = Random.Range(1, 99999).ToString();
				PlayerPrefs.SetString("user_id", userId);
			}
			SetUserName(PlayerPrefs.GetString("user_name"));
		}
		
		public void SetUserName(string userName){
			this.userName = userName;
			PlayerPrefs.SetString("user_name", userName);
			if (!string.IsNullOrEmpty(userName))
			{
				PhotonNetwork.playerName = userName + "&" + userId;
			}
		}
	}
	
	public class OtherPlayerData {
		public string userName;
		public string userId;
		public string fullName;
		
		public void Init(string data){
			fullName = data;
			string[] arr = data.Split("&"[0]);
			this.userName = arr[0];
			this.userId = arr[1];
		}
	}
	
	public class SessionData {
		public int sessionId;
		public string levelName;
		
		public void Init(int sessionId, string levelName){
			this.sessionId = sessionId;
			this.levelName = levelName;
		}
	}

	void Start () {
		player.Init();
	}
	
	public void InitSession (int sessionId) {
		string lastLevel = PlayerPrefs.GetString("last_level_with_" + otherPlayer.fullName + "_in_game_" + sessionId.ToString());
		if (string.IsNullOrEmpty(lastLevel))
		{
			lastLevel = Levels.levels[0];
		}
		session.Init(sessionId, lastLevel);
	}
	
	public void Save(){
		PlayerPrefs.SetInt("last_game_with_" + otherPlayer.fullName, session.sessionId);
		PlayerPrefs.SetString("last_level_with_" + otherPlayer.fullName + "_in_game_" + session.sessionId.ToString(), session.levelName);
	}
	
	public void SaveStringParameter(string parameterName, string parameterValue){
		PlayerPrefs.SetString(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName, parameterValue);
	}
	
	public string GetStringParameter(string parameterName){
		return PlayerPrefs.GetString(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName);
	}
	
	public void SaveIntParameter(string parameterName, int parameterValue){
		PlayerPrefs.SetInt(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName, parameterValue);
	}
	
	public int GetIntParameter(string parameterName){
		return PlayerPrefs.GetInt(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName);
	}
	
	public void SaveFloatParameter(string parameterName, float parameterValue){
		PlayerPrefs.SetFloat(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName, parameterValue);
	}
	
	public float GetFloatParameter(string parameterName){
		return PlayerPrefs.GetFloat(otherPlayer.fullName + "&" + session.sessionId + "&" + parameterName);
	}
	
	void Update () {
		
	}
}
