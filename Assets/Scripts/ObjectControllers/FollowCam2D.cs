using UnityEngine;
using System.Collections;

public class FollowCam2D : MonoBehaviour
{
	public Transform target;

	public float distance = 10.0f;
	public float extraHeight = 2.0f;
	
	bool isFixedUpdate;

	void Start () 
	{

	}

	void Update () 
	{
		if(!isFixedUpdate) UpdateCamera();
	}

	void FixedUpdate () 
	{
		if(isFixedUpdate) UpdateCamera();
	}

	void UpdateCamera()
	{
		if (target)
		{			
			Vector3 targetPos = target.position + Vector3.up * extraHeight;
			targetPos.x = -distance;
			transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
		}
	}

	public void SetTarget(GameObject inTarget, bool isFixed = false)
	{
		target = inTarget.transform;
		isFixedUpdate = isFixed;
	}

	public void UpdatePosition()
	{
		Vector3 targetPos = target.position + Vector3.up * extraHeight;
		targetPos.x = -distance;
		transform.position = targetPos;
	}
}

