using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour
{
    public GameObject FrontWheel;
    public GameObject RearWheel;


    void Start()
    {
        Vector3 center = GetComponent<Rigidbody>().centerOfMass;
        center.y -= 1;
        GetComponent<Rigidbody>().centerOfMass = center;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");

        JointMotor motor = RearWheel.GetComponent<HingeJoint>().motor;
        motor.force = (h != 0 ? 1 : 0) * 30;
        motor.freeSpin = true;
        motor.targetVelocity = -h * 4000;

        RearWheel.GetComponent<HingeJoint>().useMotor = h != 0;
        RearWheel.GetComponent<HingeJoint>().motor = motor;

        FrontWheel.GetComponent<HingeJoint>().useMotor = h != 0;
        FrontWheel.GetComponent<HingeJoint>().motor = motor;

        GetComponent<Rigidbody>().AddRelativeTorque(0, 0, -h * 3000);


        // flip the car
        if (Input.GetButton("Fire1"))
        {
            float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.z, 0);
            if (Mathf.Abs(deltaAngle) > 10)
            {
                GetComponent<Rigidbody>().AddTorque(0, 0, deltaAngle * deltaAngle * deltaAngle, ForceMode.VelocityChange);
            }
        }
    }
}
