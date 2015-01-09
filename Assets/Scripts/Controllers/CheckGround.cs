using UnityEngine;
using System.Collections;

public class CheckGround : MonoBehaviour {
	
	public GameObject[] ignoredObjects;
	[System.NonSerialized]
	public bool grounded = false;
	
	void  OnTriggerStay ( Collider col  ){
		if(col.isTrigger) return;
		foreach(GameObject obj in ignoredObjects){
			if(obj == col.gameObject) return;
		}
		grounded = true;	
	}
}