using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
	HashId hash;	
	HeadLookController headLook;
	CharacterMotor motor;
	Animator anim;
	Core core;
	GameObject cameraObject;
	Camera cameraComponent;

	bool hasControl;
	public float smooth = 10f;

	public float turnSmoothing = 15f;
	public float shiftTimeOffset = 0.4f;

	public string jumpButton = "Jump";
	public string moveButton = "Horizontal";

	private Vector3 correctPlayerPos = Vector3.zero;
	private Vector3 correctMousePos = Vector3.zero; 
	private float move = 0f;
	private bool jump = false;
	private bool onGround = false;
	private double ping = 0f;

	private float offset = 0f;
	private float hor = 0;
	private float shiftTime = 0;
	private bool shift = false;
	private bool isInitHero = false;

	void Start () 
	{
		hasControl = true;
		anim = GetComponent<Animator>();
		hash = gameObject.GetComponent<HashId>();
		headLook = gameObject.GetComponent<HeadLookController>();
		motor = GetComponent<CharacterMotor>();
		core = GameObject.Find("Administration").GetComponent<Core>();
		if(core.online){
			if(photonView.isMine){
				core.mainHero = gameObject;
			} else {
				if(core.isUpHero()) core.downHero = gameObject;
				else core.upHero = gameObject;
				core.anotherHero = gameObject;
				correctPlayerPos = transform.position;
				motor.SetScriptEnable(false);
			}
			RemoveControl();
			core.PlayerInit();
		}
		isInitHero = true;
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(isInitHero){
			if (stream.isWriting)
			{	
				PlayerStreamMe(stream, info);
			}
			else
			{
				PlayerStreamOther(stream, info);
			}
		}
	}

	void PlayerStreamMe(PhotonStream stream, PhotonMessageInfo info) {
		stream.SendNext(Input.mousePosition); 
		stream.SendNext(transform.position);
		stream.SendNext(move); 
		stream.SendNext(jump);
		stream.SendNext(motor.IsGrounded()); 
	}
	
	void PlayerStreamOther(PhotonStream stream, PhotonMessageInfo info) {
		correctMousePos = (Vector3)stream.ReceiveNext();
		correctPlayerPos = (Vector3)stream.ReceiveNext();
		move = (float)stream.ReceiveNext();
		jump = (bool)stream.ReceiveNext();
		onGround = (bool)stream.ReceiveNext();
		
		ping = PhotonNetwork.time - info.timestamp;         
	}

	void Update()
	{
		if(photonView.isMine || !core.online){
			move = Input.GetAxisRaw(moveButton);
			jump = Input.GetButton(jumpButton);
			onGround = motor.IsGrounded();
			correctMousePos = Input.mousePosition;
		} else {
			transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * smooth);
		}

		if(hasControl){
			motor.inputJump = jump;
			motor.inputMoveDirection = new Vector3(0, 0, -move);

			Movement(move, 0f, jump, onGround, correctMousePos);
		}
	}

	public void SetCamera(GameObject cam)
	{
		cameraObject = cam;
		cameraComponent = cameraObject.GetComponent<Camera>();
	}

	private void Movement(float horizontal, float vertical, bool jump, bool isGrounded, Vector3 mousePos){
		Ray cursorRay = cameraComponent.ScreenPointToRay(mousePos);
		RaycastHit hit;
		if (Physics.Raycast(cursorRay, out hit)) {
			headLook.target = hit.point + offset * Vector3.up;
		}

		anim.SetBool(hash.jumpBool, jump && !IsJumpState());
		anim.SetBool(hash.inAir, !isGrounded);
		
		if(hor != 0 && hor != horizontal && !shift){
			shiftTime = Time.time;
			shift = true;
			anim.SetBool(hash.shiftMoveBool, shift);
		}
		
		if(shift && Time.time > shiftTime + shiftTimeOffset){
			shift = false;
			anim.SetBool(hash.shiftMoveBool, shift);
		}
		
		if(horizontal != 0f){
			Rotating(0f, horizontal);
			anim.SetBool(hash.runBool, true);
		}
		else anim.SetBool(hash.runBool, false);
		
		hor = horizontal;
	}

	void Rotating (float vertical, float horizontal)
	{
		Vector3 targetDirection = new Vector3(vertical, 0f, -horizontal);
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);
		
		if(!shift){
			transform.rotation = newRotation;
		}
	}

	bool IsJumpState(){
		foreach(AnimationInfo a in anim.GetCurrentAnimationClipState(0)){
			if(a.clip.name.Contains("Jump")) return true;
		}
		return false;
	}

	public void GiveControl() { hasControl = true; }
	public void RemoveControl() { hasControl = false; }
	public bool HasControl() { return hasControl; }
}

