using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]

[System.Serializable]
public class Wheel_Options 
{
	public float dampingRatio = 0.7f;
	public float frequency = 6.0f;
	public float mass = 0.1f;
	public float gravityScale = 1.0f;
}

[System.Serializable]
public class Transmission_Options 
{
	public float GearAcceleration = 0.0f;
	public float GearMaxSpeed = 0.0f;
}

public class SimpleControlVehicle : MonoBehaviour {


	public float maxSpeed = 100.0f; // Maximum vehicle speed.
	public float acceleration = 0.15f; // Vehicle acceleration amount.
	public float breakForce = 0.1f; // Amount of vehicle braking force.
	public bool reverse = true; // Allow the reverse on the vehicle.
	public float reverseMaxSpeed = 5.0f; // Maximum reverse speed of the vehicle. 
	public float frontForce = 15.0f; // Amount of force to rotate front. 
	public float backForce = 15.0f; // Amount of force to rotate back.
	public bool wheelie = true; // Allow make wheelies. 
	public GameObject[] wheels = new GameObject[2]; // Array with all the wheels from the vehicle.
	public LayerMask whatIsGround; // LayerMask that contain ground layers.
	public Wheel_Options wheelOptions; // Parameters for the WheelJoint2D.
	public AudioClip engineSound; // Engine sound of the vehicle.
	public PhysicsMaterial2D groundMat; // Friction and bounce between the vehicle and the ground.
	public bool manualTransmission = false; //Enable the manual transmission.
	public Transmission_Options[] gears = new Transmission_Options[0]; // Array with all the gears configuration.

	private bool grounded; // Show if the vehicle is on the ground.
	private float pitchModifier; // Help to increase or decrease the pitch value of the audio.
	private float minPitch; // Minimum audio pitch value.
	private float maxPitch; // Maximum audio pitch value.
	private float horizontal; // Value of the horizontal axis.
	private float vertical; // Value of the vertical axis.
	private bool nextGear = false; // If it is true change to the next gear. 
	private bool backGear = false; // If it is true change to the next gear. 
	private bool isMobile = false; // Verify if it is a mobile platform.
	private int actualGear = 0; // The actual gear that have the vehicle.

    private SimpleControlVehicle vehicle;
    
    private GameObject startScreen;
    public bool dead = false;
    private float deathCooldown;

    void Awake(){
		// Verify if it is a mobile platform.
		#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_BLACKBERRY 
			isMobile = true;
			Debug.Log("Mobile Platform detected");
		#endif
		// If the car doesn't have a RigidBody2D we add one.
		if (!GetComponent<Rigidbody2D>())
			gameObject.AddComponent<Rigidbody2D>();
		// If the car doesn't have a AudioSource we add one.
		if (!GetComponent<AudioSource>())
			gameObject.AddComponent<AudioSource>();
		// Add the audio clip to the AudioSource if is not null
		if (engineSound)
			GetComponent<AudioSource>().clip = engineSound;
	}

	void Start (){
        vehicle = this;

        // Look all the wheels assigned on the wheels array.
        for (int i = 0; i < wheels.Length; i++) {
			// Verify if all the wheels are assigned
			if(wheels[i] == null){
				wheels = new GameObject[0];
				Debug.Log("Wheel not assigned.");
				return;
			}
			// Verify if all the wheels has a CircleCollider2D
			else if (wheels[i].GetComponent<CircleCollider2D>() == null){
				wheels = new GameObject[0];
				Debug.Log("Circle collider no assigned to the wheel.");
				return;
			}else{
				wheels[i].GetComponent<CircleCollider2D>().sharedMaterial = groundMat;
				addWheelJoint2D(i);
			}
		}
		// Assign the engine audio loop to true to repeat the sound.
		GetComponent<AudioSource>().loop = true;
		GetComponent<AudioSource>().Play ();
		minPitch = 1.0f;
		maxPitch = 4.0f;
		// If the manual transmission is enabled, set the propeties of the first gear.
		if (manualTransmission) {
			acceleration = gears[0].GearAcceleration;
			maxSpeed = gears[0].GearMaxSpeed;
		}

	}

	// Update is called every frame.
	void Update(){
        if (dead)
        {
            deathCooldown -= Time.deltaTime;

            if (deathCooldown <= 0)
            {

                startScreen = GameObject.FindGameObjectWithTag("Start");
                var deathPlayerX = vehicle.transform.position.x;
                var deathPlayerY = vehicle.transform.position.y;
                startScreen.GetComponent<SpriteRenderer>().enabled = true;

                startScreen.transform.position = new Vector3(deathPlayerX+30.0f, deathPlayerY+16.0f, 4.0f);

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    Application.LoadLevel(Application.loadedLevel);
                }
            }
        }
        else
        {
            if (!isMobile)
            {
                vertical = Input.GetAxis("Horizontal");
                horizontal = Input.GetAxis("Vertical");
                nextGear = Input.GetButtonDown("Fire1");
                backGear = Input.GetButtonDown("Fire2");
            }
            if (manualTransmission)
                ShiftGears();
            UpdateEngineSound();
            IsGrounded();
            isDeath();
        }
    }

	// This function is called every fixed framerate frame.
	void FixedUpdate (){
		AccelerateVehicle ();
		RotateVehicle ();
	}

	// Add a WheelJoint2D to the wheel contained in the array in a specific position
	public void addWheelJoint2D(int wheelPosition){
		// Add the WheelJoint2D component to the wheel
		WheelJoint2D wheelJoint = gameObject.AddComponent<WheelJoint2D>() as WheelJoint2D;
		// Verify if the wheel has a rigidbody2D.
		if(!wheels[wheelPosition].GetComponent<Rigidbody2D>())
			// Add a rigidbody2D to the wheel.
			wheels[wheelPosition].AddComponent<Rigidbody2D>();
		// Set the rigidbody2D of the car to the wheelJoint2D
		wheelJoint.connectedBody = wheels[wheelPosition].GetComponent<Rigidbody2D>();
		// Create a new JointSuspension2D and set the variables to get a cool and real suspension
		JointSuspension2D suspension = new JointSuspension2D();
		// Set the angle of the suspension to 90 degrees.
		suspension.angle = 90.0f;
		// Set the damping ratio to the configurated option.
		suspension.dampingRatio = wheelOptions.dampingRatio;
		// Set the frequency to the configurated option.
		suspension.frequency = wheelOptions.frequency;
		// Set the JointSuspension created to the WheelJoint2D
		wheelJoint.suspension = suspension;
		// Adjust the anchor to the position of the wheel.
		wheelJoint.anchor = wheels[wheelPosition].transform.localPosition;
		// Set the mass of the rigidbody2D of the wheel 
		wheels[wheelPosition].GetComponent<Rigidbody2D>().mass = wheelOptions.mass;
		// Set the gravity scale of the rigidbody2D to the wheel.
		wheels [wheelPosition].GetComponent<Rigidbody2D> ().gravityScale = wheelOptions.gravityScale;
	}

	// If the vehicle accelerate the engine sound accelerate
	public void UpdateEngineSound(){
		// Set the pitchModifier a valor that help to increase the pitch audio
		// without exceed the minimum and maximum pitch valor.
		pitchModifier = maxPitch - minPitch;
		// Set the valor of the pitch audio proportional to the actual velocity.
		GetComponent<AudioSource>().pitch = minPitch + (GetComponent<Rigidbody2D>().velocity.x / maxSpeed) * pitchModifier;
	}

	// Verify if one wheel is on the ground. If any wheel is on the ground set grounded to true.
	public void IsGrounded(){
		// Look all the array that contain the wheels.
		for(int i = 0; i < wheels.Length; i++){
			// Draw a circle of the same size of the wheel, and with the LayerMask whatIsGround
			// verify if the the circle hit the ground.
			if(Physics2D.OverlapCircle(
				wheels[i].transform.position, 
				wheels[i].GetComponent<CircleCollider2D>().radius * wheels[i].transform.localScale.x, 
				whatIsGround)){
				grounded = true;
				return;
			}
			else {
				grounded = false;
			}
		}
	}

	// Assign the velocity to the regidbody2D of the vehicle.
	public void AccelerateVehicle(){
		// Verify if the user want to move to the right and if the vehicle is on the ground.
		if(horizontal > 0 && grounded){
			// Add velocity to the rigidbody by the actual velocity plus the acceleration.
			GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x + acceleration, GetComponent<Rigidbody2D>().velocity.y);

			// Verify if the velocity added exceed the maximum speed of the vehicle.
			if(GetComponent<Rigidbody2D>().velocity.x >= maxSpeed)
				// Set the velocity to the maximum speed.
				GetComponent<Rigidbody2D>().velocity = new Vector2(maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
		}
		// Verify if the user want to move to the left and if the vehicle is on the ground.
		else if(horizontal < 0 && grounded)
		{
			// Verify if the vehicle is still moving to the front.
			if(GetComponent<Rigidbody2D>().velocity.x > acceleration){
				// Reduce the velocity adding brake force.
				GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - breakForce, GetComponent<Rigidbody2D>().velocity.y);
			}
			// Verify if the vehicle has reverse enable.
			else if (reverse){
				// Add negative acceleration to the vehicle to go in reverse.
				GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - acceleration, GetComponent<Rigidbody2D>().velocity.y);
				// Verify if the velocity of the vehicle exceed the maximum speed of the reverse.
				if(GetComponent<Rigidbody2D>().velocity.x <= (reverseMaxSpeed * -1))
					// Set the maximum speed of the vehicle.
					GetComponent<Rigidbody2D>().velocity = new Vector2(-reverseMaxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			}
		}else{
			if(GetComponent<Rigidbody2D>().velocity.x > 0 && grounded){
				GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - 0.001f, GetComponent<Rigidbody2D>().velocity.y);
				if(GetComponent<Rigidbody2D>().velocity.x < 0)
					GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
			}
		}
	}

	// Shift the gear based on the user input.
	public void ShiftGears() {
		// Verify if the user want to set the next gear.
		if (nextGear) {
			// Verify if the array have more gears.
			if(actualGear < gears.Length -1){
				actualGear++; // Change to the next gear.
				acceleration = gears[actualGear].GearAcceleration; // Set the acceleration with the gear configuration.
				maxSpeed = gears[actualGear].GearMaxSpeed; // Set the maximum speed based on the actual gear.
			}
		}else if(backGear){
			// Verify if the array have more gears.
			if(actualGear > 0){
				actualGear--; // Change to the back gear.
				acceleration = gears[actualGear].GearAcceleration; // Set the acceleration with the gear configuration.
				maxSpeed = gears[actualGear].GearMaxSpeed; // Set the maximum speed based on the actual gear.
			}
		}

	}

	// Rotate the vehicle adding torque to the rigidbody2D.
	public void RotateVehicle()
	{
		// Verify if the user want to rotate the vehicle front and if the vehicle is running.
		if((vertical > 0) && (GetComponent<Rigidbody2D>().velocity.x > acceleration * 2)){
			// Verify if the vehicle is on the air or if the wheelies are allowed when the vehicle is on the ground.
			if(!grounded || wheelie){
				// Verify if the vehicle doesn't pass the rotation force permitted.
				if(GetComponent<Rigidbody2D>().angularVelocity > (-frontForce * 10))
					// Add the front force to rotate the vehicle.
					GetComponent<Rigidbody2D>().AddTorque(-frontForce);
			}
		}
		// Verify if the user want to rotate the vehicle backwards and if the vehicle is running.
		else if((vertical < 0) && (GetComponent<Rigidbody2D>().velocity.x > acceleration * 2)){
			// Verify if the vehicle is on the air or if the wheelies are allowed when the vehicle is on the ground.
			if(!grounded || wheelie){
				// Verify if the vehicle doesn't pass the rotation force permitted.
				if(GetComponent<Rigidbody2D>().angularVelocity < (backForce * 10))
					// Add the backward force to rotate the vehicle.
					GetComponent<Rigidbody2D>().AddTorque(backForce);
			}
		}
	}
	
	// Set the horizontal axis. (Used on MobileControl script).
	public void setHorizontalAxis(int horizontal){
		this.horizontal = horizontal;
	}

	// Set the vertical axis. (Used on MobileControl script).
	public void setVerticalAxis(int vertical){
		this.vertical = vertical;
	}

	// Set the next gear to true or false. (Used on MobileControl script).
	public void setNextGear(bool nextGear){
		this.nextGear = nextGear;
	}

	// Set the back gear to true or false. (Used on MobileControl script).
	public void setBackGear(bool backGear){
		this.backGear = backGear;
	}

	// Return the actual gear of the vehicle.
	public int getActualGear(){
		return actualGear;
	}


    public void isDeath() {
        Vector3 rotation = vehicle.transform.eulerAngles;
        Debug.Log(rotation.z);
        if(rotation.z >= 89.0f && rotation.z <= 260.0f)
        {
            if (vehicle.GetComponent<Rigidbody2D>().velocity.x <= 0){
                dead = true;
                deathCooldown = 0.5f;
            }
        }
    }
}
