﻿using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class DragingObject: Photon.MonoBehaviour {
	Core core;

	public bool canDraging = true;
	public float smooth = 5f;
	public float smoothDraging = 5f;
	public float smoothRotating = 1f;

	private bool startDragging = false;
	private Vector3 correctPos = Vector3.zero;
	private Quaternion correctRot = Quaternion.identity;

	private Highlighter h;

	void Start()
	{
		h = GetComponent<Highlighter>();
		if (h == null) { h = gameObject.AddComponent<Highlighter>(); }
		h.SeeThroughOn();
		core = GameObject.Find("Administration").GetComponent<Core>();
		//rigidbody.isKinematic = !photonView.isMine && core.online;
		rigidbody.useGravity = !core.online || photonView.isMine;
		rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionX;
		correctPos = transform.position;
		correctRot = transform.rotation;
	}

	public void MouseOver()
	{
		if(!startDragging) h.On(Color.blue);
	}

	public void Rotate () {
		transform.Rotate(new Vector3(2, 0, 0));
		if(core.online) photonView.RPC("RPCRotate", PhotonTargets.Others, transform.eulerAngles);
	}

	[RPC]
	void RPCRotate(Vector3 rot, PhotonMessageInfo info)
	{
		Vector3 newRotation = Vector3.Lerp(transform.eulerAngles, rot, Time.deltaTime * smoothRotating);
		transform.eulerAngles = new Vector3(newRotation.x, transform.eulerAngles.y, transform.eulerAngles.z);
	}
	
	public void StartDraging () {
		rigidbody.useGravity = false;
		startDragging = true;
		string color = "red";
		if(core.isDownHero()) color = "green";
		if(core.online) photonView.RPC("StartDrag", PhotonTargets.All, color);
		else StartDrag(color, null);
	}

	[RPC]
	void StartDrag(string color, PhotonMessageInfo info)
	{
		rigidbody.freezeRotation = true;
		if(color == "gree") h.ConstantOn(Color.green);
		else h.ConstantOn(Color.red);
	}

	public void StopDraging () {
		startDragging = false;
		if(core.online) photonView.RPC("StopDrag", PhotonTargets.All);
		else StopDrag(null);
	}

	[RPC]
	void StopDrag(PhotonMessageInfo info)
	{
		rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionX;
		rigidbody.useGravity = !core.online || photonView.isMine;
		h.Off();
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
		rigidbody.velocity = positionChange * smoothDraging;
	}
}
