using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {
	public bool canControl = true;
	
	public float speed = 8.0f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public float inAirControl = 0.1f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;
	float lastGroundY = 0;
	public float jumpOffset = 0.2f;
	bool grounded = false;

	[System.NonSerialized]
	public float inputX = 0;
	[System.NonSerialized]
	public bool inputJump = false;
	[System.NonSerialized]
	public bool canNextJump = true;
	[System.NonSerialized]
	public bool releaseInputJump = true;

	private bool enabledScript = true;
	private bool hasCollisions = false;
	private bool lastGrounded = false;
	private float lastOnLandTime = 0;
	
	private CapsuleCollider capsule;
	private CheckGround ground;
	
	void  Awake (){
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = false;
		capsule = GetComponent<CapsuleCollider>();
		ground = GetComponentInChildren<CheckGround>();
	}

	public void SetScriptActivity(bool value)
	{
		enabled = value;
		rigidbody.isKinematic = !value;
	}
	
	void FixedUpdate (){
		if(ground != null){
			grounded = ground.grounded;
			if(grounded && !lastGrounded){
				lastOnLandTime = Time.time;
				lastGroundY = rigidbody.velocity.y;
				canNextJump = releaseInputJump;
			}
			lastGrounded = grounded;
		}
		
		if(enabledScript){
			if(canControl){
				Vector3 targetVelocity = new Vector3(0, 0, inputX * speed);
				
				Vector3 velocity = rigidbody.velocity;
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;		        
				
				if (grounded)
				{
					if (canJump && inputJump && Time.time > lastOnLandTime + jumpOffset)
					{
						releaseInputJump = false;
						rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
					} else {
						rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
					}
				}
				else
				{
					if(!hasCollisions)
						rigidbody.AddForce(velocityChange * inAirControl, ForceMode.VelocityChange);
				}
			}
			rigidbody.AddForce(new Vector3 (0, -gravity * rigidbody.mass, 0));
		}
		
		if(ground != null){
			ground.grounded = false;
			hasCollisions = false;
		}
	}
	
	public bool IsGrounded (){
		return grounded;
	}
	
	void OnCollisionStay ( Collision col  ){
		hasCollisions = true;
	}
	
	private float CalculateJumpVerticalSpeed (){
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}
		
}