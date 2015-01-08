using UnityEngine;
using System.Collections;

public class StartMenu : Photon.MonoBehaviour {
	private string userName = "";
	private DataController data;
	private Vector2 scrollPos = Vector2.zero;
	private Vector2 scrollPosGame = Vector2.zero;
	private bool connected = false;
	private bool complete = false;
	private int gamesCount = 0;
	
	private bool connectToRoom = false;
	
	void Start () {
		PhotonNetwork.automaticallySyncScene = true;
		
		if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
		{
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
		
		connected = false;
		complete = false;
		connectToRoom = false;
		gamesCount = 0;
		data = GetComponent<DataController>();
	}
	
	void OnGUI()
	{
		if(string.IsNullOrEmpty(data.player.userName)){
			GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 100), "Enter Your Name");
			
			GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 100));
			GUILayout.Space(50);
			GUILayout.BeginHorizontal();
			
			GUILayout.Space(25);
			
			userName = GUILayout.TextField(userName, GUILayout.Width(275));
			
			GUILayout.Space(25);
			
			if (GUILayout.Button("Ok", GUILayout.Width(50)))
			{
				data.player.SetUserName(userName);
			}
			
			GUILayout.EndHorizontal();
			
			GUILayout.EndArea();
			return;
		}
		if(!connected){
			GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "Join or Create a Room");
			
			GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300));
			GUILayout.Space(50);
			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Create Room", GUILayout.Width(100)))
			{
				connectToRoom = true;
				PhotonNetwork.CreateRoom(PhotonNetwork.playerName, true, true, 2);
			}
			
			GUILayout.EndHorizontal();
			GUILayout.Space(15);
			GUILayout.BeginHorizontal();
			
			GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Join Random", GUILayout.Width(100)))
			{
				connectToRoom = true;
				PhotonNetwork.JoinRandomRoom();
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(15);
			if (PhotonNetwork.GetRoomList().Length != 0)
			{
				GUILayout.Label(PhotonNetwork.GetRoomList().Length + " currently available. Join either:");
				
				this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
				foreach (RoomInfo roomInfo  in PhotonNetwork.GetRoomList())
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(roomInfo.name.Split("&"[0])[0]);
					if (GUILayout.Button("Join"))
					{
						connectToRoom = true;
						PhotonNetwork.JoinRoom(roomInfo.name);
					}
					
					GUILayout.EndHorizontal();
				}
				
				GUILayout.EndScrollView();
			}
			
			GUILayout.EndArea();
			return;
		}
		if(!complete){
			GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 50), "Waiting Player");
			return;
		}
		if(gamesCount != 0){
			GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "Game");
			GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300));
			
			GUILayout.Space(15);
			
			if (GUILayout.Button("Continue Game", GUILayout.Width(100)))
			{
				ContinueGame(true);
			}
			
			GUILayout.Space(15);
			
			if (GUILayout.Button("New Game", GUILayout.Width(100)))
			{
				NewGame(true);
			}
			
			GUILayout.Space(15);
			if (gamesCount > 1)
			{	
				this.scrollPosGame = GUILayout.BeginScrollView(this.scrollPosGame);
				for (int i = 0; i < gamesCount; i++)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(PlayerPrefs.GetString("last_level_with_" + data.otherPlayer.fullName + "_in_game_" + i.ToString()));
					if (GUILayout.Button("Continue"))
					{
						StartGame(i, true);
					}
					
					GUILayout.EndHorizontal();
				}
				
				GUILayout.EndScrollView();
			}
			
			GUILayout.Space(15);
			
			GUILayout.EndArea();
			return;
		}
	}
	
	[RPC]
	void SendNewGame(PhotonMessageInfo info)
	{
		gamesCount++;
		PlayerPrefs.SetInt("games_with_" + data.otherPlayer.fullName, gamesCount);
		SendStartGame(gamesCount - 1, info);
	}
	
	[RPC]
	void SendStartGame(int gameId, PhotonMessageInfo info)
	{
		data.InitSession(gameId);
		data.Save();
		PhotonNetwork.LoadLevel(data.session.levelName);
	}
	
	void Update () {
		if(connected && PhotonNetwork.otherPlayers.Length == 1 && !complete){
			complete = true;
			data.otherPlayer.Init(PhotonNetwork.otherPlayers[0].ToString());
			gamesCount = PlayerPrefs.GetInt("games_with_" + data.otherPlayer.fullName);
			if(gamesCount == 0){
				NewGame(true);
			}
		}
	}
	
	void ContinueGame(bool toAll){
		StartGame(PlayerPrefs.GetInt("last_game_with_" + data.otherPlayer.fullName), toAll);
	}
	
	void NewGame(bool toAll){
		if(toAll){
			photonView.RPC("SendNewGame", PhotonTargets.All);
		} else {
			SendNewGame(null);
		}	
	}
	
	void StartGame(int gameId, bool toAll){
		if(toAll){
			photonView.RPC("SendStartGame", PhotonTargets.All, gameId);
		} else {
			SendStartGame(gameId, null);
		}	
	}
	
	void OnJoinedRoom()
	{
		connected = true;
		Debug.Log("OnJoinedRoom");
	}
	
	void OnCreatedRoom()
	{
		connected = true;
		Debug.Log("OnCreatedRoom");
	}
	
	void OnDisconnectedFromPhoton()
	{
		Debug.Log("Disconnected from Photon.");
	}
	
	void OnFailedToConnectToPhoton(object parameters)
	{
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
	}
}
