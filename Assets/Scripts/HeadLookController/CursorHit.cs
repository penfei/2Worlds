using UnityEngine;
using System.Collections;

public class CursorHit : MonoBehaviour {
	
	private float offset = 0f;
	
	void LateUpdate () {
				
		Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(cursorRay, out hit)) {
			transform.position = hit.point + offset * Vector3.up;
		}
	}
}
