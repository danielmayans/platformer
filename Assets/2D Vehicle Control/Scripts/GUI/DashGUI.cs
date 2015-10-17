using UnityEngine;
using System.Collections;

public class DashGUI : MonoBehaviour {

	public SimpleControlVehicle vehicleControl; // Used to get the speed of the vehicle.
	public GameObject speedArrow; // Used to rotate the arrow.
	public 	GUIText speed; // Text with the actual speed of the vehicle.
	public GUIText kmText; // Text with the text Km/H
	public GUIText gear; // Text with the atual gear.
	public Transform speedPanel;  // Used as pivot to align the text of speed and gear in runtime.
	public Vector3 speedOffset = new Vector3(-0.05f,0.25f,0);    // Offset of the speed text based on the pivot.
	public Vector3 kmOffset = new Vector3(-0.05f,-0.05f,0);    // Offset of the km/h text based on the pivot.
	public Vector3 gearOffset = new Vector3(-0.05f,-0.9f,0); // Offset of the gear text based on the pivot.

	// Use this for initialization
	void Start () {
		if(!vehicleControl){
			Debug.Log("Simple Control Vehicle script not assigned.");
		}
		if(!Camera.main){
			Debug.Log("Main camera not assigned.");
		}
	}

	// Update is called once per frame
	void Update () {
		//Set the actual gear to the GUIText
		gear.text = "" + (vehicleControl.getActualGear ()+1);
		// Verify if the velocity of the vehicle is less than 0 to set the actual speed.
		if(vehicleControl.GetComponent<Rigidbody2D>().velocity.x < 0)
			speed.text = "0" ;
		else
			speed.text =((int)(vehicleControl.GetComponent<Rigidbody2D>().velocity.x) * 5 ) + "";
		// Obtain the rotation of the arrow based on the actual speed.
		Quaternion rotation = Quaternion.AngleAxis(
			(vehicleControl.GetComponent<Rigidbody2D>().velocity.x / vehicleControl.maxSpeed) * -260,
			Vector3.forward);
		if (rotation.z < 0)
			speedArrow.transform.rotation = rotation;
		// Align all the GUIText based on the speed panel and offsets.
		speed.gameObject.transform.position = Camera.main.WorldToViewportPoint(speedPanel.position + speedOffset);
		kmText.gameObject.transform.position = Camera.main.WorldToViewportPoint(speedPanel.position + kmOffset);
		gear.gameObject.transform.position = Camera.main.WorldToViewportPoint(speedPanel.position + gearOffset);
	}
}
