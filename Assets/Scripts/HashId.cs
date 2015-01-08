using UnityEngine;
using System.Collections;

public class HashId : MonoBehaviour {
	public int runBool;
	public int jumpBool;
	public int shiftMoveBool;
	public int inAir;
	
	void Awake ()
	{
		runBool = Animator.StringToHash("Run");
		jumpBool = Animator.StringToHash("Jump");
		shiftMoveBool = Animator.StringToHash("ShiftMove");
		inAir = Animator.StringToHash("InAir");
	}
}
