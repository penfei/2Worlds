using UnityEngine;
using System.Collections;

public class PlayerController : Photon.MonoBehaviour
{
	HeadLookController headLook;
	CharacterMotor motor;
	Animator anim;
	Core core;
	LeftHandController leftHandController;
	GameObject cameraObject;
	Camera cameraComponent;

	Core.CharacterType heroType = Core.CharacterType.Not;

	bool hasControl;
	public float smooth = 10f;
	public float turnSmoothing = 15f;
	public float shiftTimeOffset = 0.4f;
	public float perspective = 0.008f;
	public LayerMask targetingLayerMask = -1;

	[System.NonSerialized]
	public string jumpButton = "Jump";
	[System.NonSerialized]
	public string moveButton = "Horizontal";
	[System.NonSerialized]
	public string objectButton = "Fire1";
	[System.NonSerialized]
	public Vector3 correctMousePos = Vector3.zero;

	private Vector3 correctPlayerPos = Vector3.zero;
	private Vector3 objectTargetPosition = Vector3.zero;
	private GameObject objectTarget; 
	private float move = 0f;
	private bool jump = false;
	private bool objectActive = false;
	private bool onGround = false;
	private double ping = 0f;
	
	private float hor = 0;
	private float shiftTime = 0;
	private bool shift = false;
	private bool isInitHero = false;
	private GameObject pObjectTarget;

	private Vector3 raycast = Vector3.zero;
	private Vector3 perspectiveOffset = Vector3.zero;

	void Start () 
	{
		hasControl = true;
		anim = GetComponent<Animator>();
		headLook = gameObject.GetComponent<HeadLookController>();
		motor = GetComponent<CharacterMotor>();
		leftHandController = GetComponentInChildren<LeftHandController>();
		core = GameObject.Find("Administration").GetComponent<Core>();

		if(core.online){
			if(photonView.isMine){
				core.mainHero = gameObject;
			} else {
				if(core.isUpHero()) core.downHero = gameObject;
				else core.upHero = gameObject;
				core.anotherHero = gameObject;
				correctPlayerPos = transform.position;
				motor.SetScriptActivity(false);
			}
			RemoveControl();
			core.PlayerInit();
		}
		isInitHero = true;
	}

	public void SetCharacterType(Core.CharacterType type)
	{
		heroType = type;
	}

	public void SetColor(Color color)
	{
		SkinnedMeshRenderer render = GetComponentInChildren<SkinnedMeshRenderer>();
		foreach(Material mat in render.materials){
			mat.color = color;
		}
	}

	void OnAnimatorIK(int layerIndex)
	{	
		if(layerIndex == 1) leftHandController.OnAnimatorIK();
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
		stream.SendNext(correctMousePos); 
		stream.SendNext(transform.position);
		stream.SendNext(move); 
		stream.SendNext(jump);
		stream.SendNext(motor.IsGrounded());
		stream.SendNext(objectTargetPosition);
	}
	
	void PlayerStreamOther(PhotonStream stream, PhotonMessageInfo info) {
		correctMousePos = (Vector3)stream.ReceiveNext();
		correctPlayerPos = (Vector3)stream.ReceiveNext();
		move = (float)stream.ReceiveNext();
		jump = (bool)stream.ReceiveNext();
		onGround = (bool)stream.ReceiveNext();
		objectTargetPosition = (Vector3)stream.ReceiveNext();
		
		ping = PhotonNetwork.time - info.timestamp;         
	}

	void Update()
	{
		if(core.isInited()){
			if(photonView.isMine || !core.online){
				correctMousePos = GetCorrectMousePosition();
				move = Input.GetAxisRaw(moveButton);
				motor.releaseInputJump = !Input.GetButton(jumpButton);
				if(!Input.GetButton(jumpButton)) motor.canNextJump = true;
				jump = Input.GetButton(jumpButton) && motor.canNextJump;
				objectActive = Input.GetButton(objectButton);
				objectTargetPosition = GetObjectTargetPosition();
				onGround = motor.IsGrounded();
			} else {
				transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * smooth);
			}

			motor.inputJump = jump;
			leftHandController.objectTargetPosition = objectTargetPosition;
			motor.inputX = -move;
			
			Movement(move, 0f, jump, onGround, correctMousePos);
		}
	}

	public Vector3 GetCorrectMousePosition()
	{
		Ray cursorRay = cameraComponent.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(cursorRay, out hit)) {
			raycast = hit.point;
		}

		if (Physics.Raycast(cursorRay, out hit, Mathf.Infinity, targetingLayerMask)) {
			pObjectTarget = hit.transform.gameObject;
		} else {
			pObjectTarget = null;
		}
		if(pObjectTarget != null){
			DragingObject drag = pObjectTarget.GetComponent<DragingObject>();
			if(drag != null && drag.canDraging) drag.MouseOver();
			else pObjectTarget = null;
		}

		if(heroType == Core.CharacterType.Up) perspectiveOffset = new Vector3(0, Screen.height * 0.75f - Input.mousePosition.y, Input.mousePosition.x - Screen.width * 0.5f);
		else perspectiveOffset = new Vector3(0, Screen.height * 0.25f - Input.mousePosition.y, Input.mousePosition.x - Screen.width * 0.5f);
		perspectiveOffset *= perspective;

		return raycast - perspectiveOffset;
	}

	public void SetCamera(GameObject cam)
	{
		cameraObject = cam;
		cameraComponent = cameraObject.GetComponent<Camera>();
	}

	private Vector3 GetObjectTargetPosition(){
		if(objectActive && objectTarget != null) return objectTarget.transform.position;
		else if(!objectActive && objectTarget != null) return ResetObject();
		else if(objectActive && objectTarget == null) return FindTargetObject();
		return Vector3.zero;
	}

	private Vector3 ResetObject(){
		objectTarget.GetComponent<DragingObject>().StopDraging();
		objectTarget = null;
		return Vector3.zero;
	}

	private Vector3 FindTargetObject(){
		objectTarget = pObjectTarget;
		if(objectTarget == null) return Vector3.zero;
		objectTarget.GetComponent<DragingObject>().StartDraging();
		return objectTarget.transform.position;
	}

	private void Movement(float horizontal, float vertical, bool jump, bool isGrounded, Vector3 mousePos){
		headLook.target = mousePos;

		anim.SetBool("Jump", jump && !IsJumpState());
		anim.SetBool("InAir", !isGrounded);
		
		if(hor != 0 && hor != horizontal && !shift){
			shiftTime = Time.time;
			shift = true;
			anim.SetBool("ShiftMove", shift);
		}
		
		if(shift && Time.time > shiftTime + shiftTimeOffset){
			shift = false;
			anim.SetBool("ShiftMove", shift);
		}
		
		if(horizontal != 0f){
			Rotating(0f, horizontal);
			anim.SetBool("Run", true);
		}
		else anim.SetBool("Run", false);
		
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

