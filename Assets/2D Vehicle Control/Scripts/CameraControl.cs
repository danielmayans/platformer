using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public float dampTime = 0.3f; // Offset of the camera to follow the target.
	public Vector3 offset = Vector3.zero; // Offset of the position of the camera to the target.
	private Vector3 velocity = Vector3.zero; //Velocity of the smooth damp function.
	public Transform target; // The object that camera have to follow.
	
	// Update is called once per frame
	void FixedUpdate () {
		// Verify if the target is not null
		if(target) {
			Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
			Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
			// Set the destination of the camera with the offset.
			Vector3 destination = transform.position + delta - offset;
			
			// 	Set the position of the camera smooth to the target position.
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
		}
	}
}
