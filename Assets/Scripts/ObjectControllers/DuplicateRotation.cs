using UnityEngine;
using System.Collections;

public class DuplicateRotation : MonoBehaviour {

	public Transform target;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(target != null)
		{
			transform.rotation = target.rotation;
		}
	}
}
