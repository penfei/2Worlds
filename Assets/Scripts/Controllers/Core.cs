using UnityEngine;
using System.Collections;

public class Core : Photon.MonoBehaviour {
	public bool online = true;
	public Transform heroPrefab;
	public Transform cameraPrefab;

	public Transform upHeroPostion;
	public Transform downHeroPostion;

	public GameObject upHero;
	public GameObject downHero;
	public GameObject mainHero;
	public GameObject anotherHero;

	GameDataController data;
	int initedPrefabs = 0;
	
	enum CharacterType { Not, Up, Down};
	CharacterType heroType = CharacterType.Not;
	// Use this for initialization
	void Awake () {
		if (!PhotonNetwork.connected && online)
		{
			Application.LoadLevel(Levels.MainMenu);
			return;
		}
		if(!online){
			upHero = ((Transform)Instantiate(heroPrefab, upHeroPostion.position, new Quaternion(0, -0.05f, 0, 1))).gameObject;
			downHero = ((Transform)Instantiate(heroPrefab, downHeroPostion.position, new Quaternion(0, -0.05f, 0, 1))).gameObject;
			PlayerController secondHeroController = downHero.GetComponent<PlayerController>();
			secondHeroController.jumpButton = "Jump2";
			secondHeroController.moveButton = "Horizontal2";
			secondHeroController.objectButton = "Object2";

			InitCamera(upHero, 0.5f, true);
			InitCamera(downHero, 0, true);
		} else {
			data = GetComponent<GameDataController>();
		}
	}

	void Start()
	{
		if(online){
			data.StartLevel(PhotonNetwork.otherPlayers[0].ToString());
		}
	}

	public bool isInited(){
		return initedPrefabs == 2 || !online;
	}

	public bool isUpHero()
	{
		return heroType == CharacterType.Up;
	}

	public bool isDownHero()
	{
		return heroType == CharacterType.Down;
	}

	public bool isNotChoicedHero()
	{
		return heroType == CharacterType.Not;
	}
	
	void Update () {
	
	}

	void OnGUI()
	{
		if(online) OnlineOnGUI();		
	}

	void OnlineOnGUI()
	{
		if (GUI.Button (new Rect(10,10,100,30),"Return to Loby"))
		{
			PhotonNetwork.LeaveRoom();
		}
		if(isNotChoicedHero()){
			if (GUI.Button (new Rect(10,50,100,30),"Up")){
				InitUpHero(null);
				photonView.RPC("InitDownHero", PhotonTargets.Others);
			}
			if (GUI.Button (new Rect(10,90,100,30),"Down")){
				InitDownHero(null);
				photonView.RPC("InitUpHero", PhotonTargets.Others);
			}
		}
	}

	[RPC]
	void InitUpHero(PhotonMessageInfo info)
	{
		heroType = CharacterType.Up;
		upHero = PhotonNetwork.Instantiate(this.heroPrefab.name, upHeroPostion.position, Quaternion.identity, 0);
	}

	[RPC]
	void InitDownHero(PhotonMessageInfo info)
	{
		heroType = CharacterType.Down;
		downHero = PhotonNetwork.Instantiate(this.heroPrefab.name, downHeroPostion.position, Quaternion.identity, 0);
	}

	public void PlayerInit()
	{
		initedPrefabs++;

		if(isInited()){
			InitCamera(downHero, 0, downHero.GetComponent<PlayerController>().photonView.isMine);
			InitCamera(upHero, 0.5f, upHero.GetComponent<PlayerController>().photonView.isMine);

			upHero.GetComponent<PlayerController>().GiveControl();
			downHero.GetComponent<PlayerController>().GiveControl();
		}
	}

	void InitCamera(GameObject hero, float offsetY, bool isFixedUpdate)
	{
		Transform camTr = (Transform)Instantiate(cameraPrefab);
		GameObject cam = camTr.gameObject;
		cam.GetComponent<FollowCam2D>().SetTarget(hero, isFixedUpdate);
		cam.GetComponent<FollowCam2D>().UpdatePosition();
		cam.GetComponent<Camera>().rect = new Rect(0, offsetY, 1, 0.5f);
		hero.GetComponent<PlayerController>().SetCamera(cam);
	}

	void OnMasterClientSwitched(PhotonPlayer player)
	{
		Debug.Log("OnMasterClientSwitched: " + player);
	}
	
	void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom (local)");
		
		Application.LoadLevel(Levels.MainMenu);
	}
	
	void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton");
		
		Application.LoadLevel(Levels.MainMenu);
	}
	
	void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
	}
	
	void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("OnPlayerDisconneced: " + player);
		
		PhotonNetwork.LeaveRoom();
	}
	
	void OnFailedToConnectToPhoton()
	{
		Debug.Log("OnFailedToConnectToPhoton");
		
		Application.LoadLevel(Levels.MainMenu);
	}
}
