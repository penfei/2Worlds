using UnityEngine;
using System.Collections;

public class LeftHandController : MonoBehaviour {

	public GameObject player;
	public GameObject targetObject;
	private GameObject targetObjectHand;
	
	public float handWeight = 0f;
	public float handWeightMax = 1f;
	public float fieldOfViewAngle = 150f;
	public float smoothUp = 4f;
	public float smoothDown = 10f;
	public float smoothTarget = 8f;

	[System.NonSerialized]
	public bool playerActive = false;
	[System.NonSerialized]
	public bool isButtonUp = false;
	[System.NonSerialized]
	public bool inRadius = false;
	[System.NonSerialized]
	public bool targetInSight = false;
	[System.NonSerialized]
	public bool targetFirst = false;

	private Animator anim;

	void Start () {
		targetObject = GameObject.Find("LeftHandCube");
		targetObjectHand = targetObject;
		anim = player.GetComponent<Animator>();
	}

	void Update () {
		anim.SetBool("Object", playerActive);
	}

	bool CanPulling () {
		return playerActive && isButtonUp;
	}

	public void OnAnimatorIK(int layerIndex)
	{	
		if((CanPulling () || handWeight > 0) && layerIndex == 1)
		{	
			if(CanPulling () && handWeight != handWeightMax){
				handWeight = Mathf.Lerp(handWeight, handWeightMax, Time.deltaTime * smoothUp);
			}

			Vector3 target = targetObjectHand.transform.position;
			
			if(!targetInSight && targetObject != null){
				target = new Vector3(-4000, 0, 0);
			}
			
			anim.SetIKPosition(AvatarIKGoal.LeftHand, target);
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight); 
		}
		if(!CanPulling () && layerIndex == 1){
			if(handWeight != 0){
				handWeight = Mathf.Lerp(handWeight, 0, Time.deltaTime * smoothDown);
			}
			if(handWeight < 0.01){
				handWeight = 0;
			}
		}
	}

	void FixedUpdate(){
		if(!playerActive && !isButtonUp){
			isButtonUp = true;
		}
		if(targetObject != null && (CanPulling() || handWeight > 0)){
			targetInSight = false;
			targetFirst = false;
			
			Vector3 direction = targetObject.transform.position - player.transform.position;
			float angle = Vector3.Angle(direction, player.transform.forward);
			
			if(angle < fieldOfViewAngle * 0.5f)
			{
				targetInSight = true;
			}
			RaycastHit hit;
			if(Physics.Raycast(player.transform.position, direction, out hit))
			{
				if(hit.collider.gameObject == targetObject)
				{
					targetFirst = true;
				}
			}
			inRadius = targetObject && Vector3.Distance(player.transform.position, targetObject.transform.position) < gameObject.GetComponent<SphereCollider>().radius;
		}
	}
}
