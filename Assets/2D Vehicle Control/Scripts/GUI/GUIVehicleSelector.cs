using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CameraControl))]

public class GUIVehicleSelector : MonoBehaviour {
	
	public GameObject policeCar;
	public GameObject redCar;
	public GameObject grayCar;
	public GameObject speedometer;

	private GameObject currentCar; // The vehicle that is controlled.
	bool showControls = false;
	void Start(){
		if(policeCar && redCar && grayCar && speedometer){
			// Disable all the vehicles except the red car.
			speedometer.SetActive(false);
			policeCar.GetComponent<AudioSource>().enabled = false;
			grayCar.GetComponent<AudioSource>().enabled = false;
			policeCar.GetComponent<SimpleControlVehicle> ().enabled = false;
			grayCar.GetComponent<SimpleControlVehicle> ().enabled = false;
			currentCar = redCar;
			gameObject.GetComponent<CameraControl>().target = currentCar.transform;
		}
	}

	void Update(){
		// Restart the rotation of the vehicle to the initial position.
		if (Input.GetButtonDown ("Jump"))
			currentCar.transform.rotation = Quaternion.identity;
	}

	void OnGUI() {
		if(policeCar && redCar && grayCar){			
			// Create a button to select the red car.
			if (GUI.Button (new Rect (10, 10, 100, 50), "Red Car")) {
				speedometer.SetActive(false);
				redCar.GetComponent<AudioSource>().enabled = true;
				grayCar.GetComponent<AudioSource>().enabled = false;
				policeCar.GetComponent<AudioSource>().enabled = false;
				redCar.GetComponent<SimpleControlVehicle> ().enabled = true;
				grayCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				policeCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				currentCar = redCar;
				gameObject.GetComponent<CameraControl>().target = currentCar.transform;
			}
			// Create a button to select the police car.
			if (GUI.Button (new Rect (120, 10, 100, 50), "Police-Manual")) {
				speedometer.SetActive(true);
				redCar.GetComponent<AudioSource>().enabled = false;
				grayCar.GetComponent<AudioSource>().enabled = false;
				policeCar.GetComponent<AudioSource>().enabled = true;
				redCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				grayCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				policeCar.GetComponent<SimpleControlVehicle> ().enabled = true;
				currentCar = policeCar;
				gameObject.GetComponent<CameraControl>().target = currentCar.transform;
			}
			// Create a button to select the gray car.
			if (GUI.Button (new Rect (230, 10, 100, 50), "Gray Car")) {
				speedometer.SetActive(false);
				redCar.GetComponent<AudioSource>().enabled = false;
				grayCar.GetComponent<AudioSource>().enabled = true;
				policeCar.GetComponent<AudioSource>().enabled = false;
				redCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				grayCar.GetComponent<SimpleControlVehicle> ().enabled = true;
				policeCar.GetComponent<SimpleControlVehicle> ().enabled = false;
				currentCar = grayCar;
				gameObject.GetComponent<CameraControl>().target = currentCar.transform;
			}
			if (GUI.Button (new Rect (340, 10, 100, 50), "Controls")) {
				showControls = !showControls;

			}
			if(showControls){
				GUI.Box(new Rect(Screen.width / 2 - 150, 70, 300, 150), "\nControls:\n"
				        + "Right Arrow: Forwards\nLeft Arrow: Reverse\nDown Arrow: Tilt Backwards\n" +
				        "Up Arrow: Tilt Forwards\nSpace: Restart Vehicle.\nControl: Next gear.\nAlt: Previous Gear.");
			}
		}
		
	}
}
