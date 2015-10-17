using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SimpleControlVehicle))]

public class MobileControl : MonoBehaviour {
	
	public bool accelerometer = false; // Control the vertical axis by accelerometer.
	public float sensibility = 0.2f; // Sensibility of the accelerometer.
	public Collider2D forwardsBtn; // Collider2D of the forward button.
	public Collider2D backwardsBtn; // Collider2D of the backwards button.
	public Collider2D tiltForwardsBtn; // Collider2D of the tilt forwards button.
	public Collider2D tiltBackwardsBtn; // Collider2D of the tilt backwards button.
	public Collider2D nextGearsBtn; // Collider2D of the next gear button.
	public Collider2D previousGearBtn; // Collider2D of the previous gear button.
	public Collider2D restartBtn; // Collider2D of the restart button.

	private SimpleControlVehicle vehicleScript;

	void Awake(){
		// Get the SimpleControlVehicle script of the game object.
		vehicleScript = gameObject.GetComponent<SimpleControlVehicle> ();
	}

	void Start (){
		if(forwardsBtn == null || backwardsBtn == null || tiltForwardsBtn == null ||
		   tiltBackwardsBtn == null || restartBtn == null){
			Debug.Log("Collider2D not assigned.");
		}
	}

	// Update is called every frame.
	void Update(){
		// Verify if the user touch the screen.
		if(Input.touches.Length > 0)
		{
			Vector2 touchPos;
			// Look all the touches 
			for(int i = 0; i < Input.touches.Length; i++){
				// Convert the touch position on the screen to the world position.
				Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
				touchPos = new Vector2(worldPoint.x, worldPoint.y);
				HorizontalDirection(touchPos, i);
				GearInput(touchPos, i);
				// Verify if the accelerometer is active.
				if(!accelerometer)
					VerticalDirection(touchPos, i);
				restartButton(touchPos, i);
			}
			
		}
		// Verify if the accelerometer is active.
		if (accelerometer)
			VerticalDirectionAccelerometer ();
	}

	// Verify if the user touch a forward or backward button and update the SimpleControlVehicle.
	public void HorizontalDirection(Vector2 touchPosition, int position){

		// Verify if the fordward button is touched.
		if (forwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Began)
		{
			vehicleScript.setHorizontalAxis(1);
		// Verify if the forward button is released.
		}else if (forwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended){
			vehicleScript.setHorizontalAxis(0);
		}
		// Verify if the backward button is touched.
		if(backwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Began){
			vehicleScript.setHorizontalAxis(-1);
		// Verify if the backward button is released.
		}else if(backwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended){
			vehicleScript.setHorizontalAxis(0);
		}
	}

	// Verify if the user touch a tilt button and update the SimpleControlVehicle.
	public void VerticalDirection(Vector2 touchPosition, int position){
		// Verify if the tilt forward button is touched.
		if (tiltForwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Began)
		{
			vehicleScript.setVerticalAxis(1);
		// Verify if the tilt forward button is released.
		}else if (tiltForwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended){
			vehicleScript.setVerticalAxis(0);
		}
		// Verify if the tilt backward button is touched.
		if(tiltBackwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Began){
			vehicleScript.setVerticalAxis(-1);
		// Verify if the tilt backward button is released.
		}else if(tiltBackwardsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended){
			vehicleScript.setVerticalAxis(0);
		}
				
	}

	// Verify the position of the mobile and update the SimpleControlVehicle.
	public void VerticalDirectionAccelerometer(){
		// If the tilt forward of the mobile is more than the sensibility 
		// the vehicle rotate forward.
		if(Input.acceleration.x > sensibility){
			vehicleScript.setVerticalAxis(1);
		// If the tilt backwards of the mobile is more than the sensibility 
		// the vehicle rotate backward.
		}else if(Input.acceleration.x < -sensibility){
			vehicleScript.setVerticalAxis(-1);
		}else {
			vehicleScript.setVerticalAxis(0);
		}
	}

	// Verify if the user touch a gear button and update the SimpleControlVehicle.
	public void GearInput(Vector2 touchPosition, int position){
		// Verify if the next gear button is touched.
		if (nextGearsBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended)
		{
			vehicleScript.setNextGear(true);
		}else{
			vehicleScript.setNextGear(false);
		}
		// Verify if the tilt backward button is touched.
		if(previousGearBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended){
			vehicleScript.setBackGear(true);
			// Verify if the tilt backward button is released.
		}else{
			vehicleScript.setBackGear(false);
		}
		
	}

	// The vehicle back to the original position when the reset button is touched.
	public void restartButton(Vector2 touchPosition, int position){
		// Verify if the reset button is touched.
		if (restartBtn == Physics2D.OverlapPoint(touchPosition) && Input.GetTouch(position).phase == TouchPhase.Ended)
		{
			gameObject.transform.rotation = Quaternion.identity;
		}
	}

}
