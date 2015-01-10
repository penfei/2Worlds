using UnityEngine;
using System.Collections;

public class LeftHandController : MonoBehaviour {

	public GameObject player;
	public Vector3 objectTargetPosition = Vector3.zero;
	
	public float handWeight = 0f;
	public float handWeightMax = 1f;
	public float fieldOfViewAngle = 150f;
	public float smoothUp = 4f;
	public float smoothDown = 10f;
	public float smoothTarget = 8f;

	private Animator anim;
	private Vector3 target;
	private Vector3 handTarget;
	private Vector3 lastTarget;

	void Start () {
		anim = player.GetComponent<Animator>();
	}

	void Update () {
		anim.SetBool("Object", objectTargetPosition != Vector3.zero);
	}

	public void OnAnimatorIK()
	{	
		if(objectTargetPosition != Vector3.zero || handWeight > 0)
		{	
			target = objectTargetPosition;
			if(Vector3.Angle(target - player.transform.position, player.transform.forward) > fieldOfViewAngle * 0.5f)
			{
				if(player.transform.eulerAngles.y < 315 && player.transform.eulerAngles.y > 160) target = new Vector3(player.transform.position.x + 10f, transform.position.y, transform.position.z);
				else target = new Vector3(player.transform.position.x - 10f, transform.position.y, transform.position.z);
			}
			if(objectTargetPosition == Vector3.zero)
			{
				target = lastTarget;
			}
			if(anim.GetBool("Run")) target.y = transform.position.y;
			lastTarget = target;


			handTarget = Vector3.Lerp(handTarget, target, Time.deltaTime * smoothTarget);

			if(objectTargetPosition != Vector3.zero && handWeight != handWeightMax){
				handWeight = Mathf.Lerp(handWeight, handWeightMax, Time.deltaTime * smoothUp);
			}
			
			anim.SetIKPosition(AvatarIKGoal.LeftHand, handTarget);
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight); 
		}
		if(objectTargetPosition == Vector3.zero){
			if(handWeight != 0){
				handWeight = Mathf.Lerp(handWeight, 0, Time.deltaTime * smoothDown);
			}
			if(handWeight < 0.01){
				handWeight = 0;
				handTarget = gameObject.transform.position;
			}
		}
	}
}
