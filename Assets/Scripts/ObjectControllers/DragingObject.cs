using UnityEngine;
using System.Collections;

public class DragingObject: Photon.MonoBehaviour {
	Core core;

	public bool canDraging = true;
	public float smooth = 5f;
	public float smoothDraging = 5f;

	private bool startDragging = false;
	private Vector3 correctPos = Vector3.zero;
	private Quaternion correctRot = Quaternion.identity;

	void Start()
	{
		core = GameObject.Find("Administration").GetComponent<Core>();
		//rigidbody.isKinematic = !photonView.isMine && core.online;
		rigidbody.useGravity = !core.online || photonView.isMine;
		correctPos = transform.position;
		correctRot = transform.rotation;
	}
	
	public void StartDraging () {
		rigidbody.useGravity = false;
		rigidbody.freezeRotation = true;
		startDragging = true;
	}

	public void StopDraging () {
		startDragging = false;
		rigidbody.freezeRotation = false;
		rigidbody.useGravity = !core.online || photonView.isMine;
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(transform.position); 
			stream.SendNext(transform.rotation);
		}
		else
		{
			correctPos = (Vector3)stream.ReceiveNext();
			correctRot = (Quaternion)stream.ReceiveNext();
		}
	}

	void Update () {
		if(core.isInited()){
			if(startDragging){
				if(core.online) photonView.RPC("Drag", PhotonTargets.All, core.mainHero.GetComponent<PlayerController>().correctMousePos);
				else Drag(core.upHero.GetComponent<PlayerController>().correctMousePos, null);
			} else {
				if(core.online && !photonView.isMine)
				{
					transform.position = Vector3.Lerp(transform.position, correctPos, Time.deltaTime * smooth);
					transform.rotation = Quaternion.Lerp(transform.rotation, correctRot, Time.deltaTime * smooth);
				}
			}
		}
	}

	[RPC]
	void Drag(Vector3 mousePosition, PhotonMessageInfo info)
	{
		Vector3 positionChange = mousePosition - transform.position;
		//rigidbody.AddForce(positionChange * smooth, ForceMode.VelocityChange);
		rigidbody.velocity = positionChange * smoothDraging;
		//rigidbody.angularVelocity *= 0.9f;
	}
}
