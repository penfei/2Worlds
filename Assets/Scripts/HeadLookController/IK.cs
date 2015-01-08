 /// <summary>
/// 
/// </summary>

using UnityEngine;
using System;
using System.Collections;
  
[RequireComponent(typeof(Animator))]  

//Name of class must be name of file as well

public class IK : MonoBehaviour {
	
	protected Animator avatar;
	
	public bool ikActive = true;
	
	public GameObject leftHandObject;
	
	public float leftHandWeightPosition = 1;
	public float leftHandWeightRotation = 1;
	
	private HeadLookController headLook;
	
	// Use this for initialization
	void Start () 
	{
		avatar = GetComponent<Animator>();
		headLook = gameObject.GetComponent<HeadLookController>();
	}
	
	void OnAnimatorIK(int layerIndex)
	{	
		
		if(ikActive)
		{	
			avatar.SetLookAtPosition(headLook.target);
	        avatar.SetLookAtWeight(1, 0.5f, 0.5f, 0.0f, 0.5f); 
			
			avatar.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObject.transform.position);
			avatar.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
//	        avatar.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObject.transform.rotation);
//	        avatar.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.5f);
			
//			Debug.Log(headLook.target);
			
//			Vector3 target = leftHandObject.transform.position;
//			avatar.SetIKPositionWeight(AvatarIKGoal.LeftHand,leftHandWeightPosition);
//			avatar.SetIKRotationWeight(AvatarIKGoal.LeftHand,leftHandWeightRotation);
				
//			avatar.SetIKPosition(AvatarIKGoal.LeftHand,headLook.target);
//			avatar.SetIKPosition(AvatarIKGoal.LeftHand,target);
		}
	}
	
	void Update () 
	{
		
	}   		  
}
