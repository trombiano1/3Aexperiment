// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    [Tooltip("Maximum steering angle of the wheels")]
    public float maxAngle = 30f;
    [Tooltip("Maximum torque applied to the driving wheels")]
    public float maxTorque = 300f;
    [Tooltip("Maximum brake torque applied to the driving wheels")]
    public float brakeTorque = 30000f;
    [Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
    public GameObject wheelShape;

    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    public float criticalSpeed = 5f;
    [Tooltip("Simulation sub-steps when the speed is above critical.")]
    public int stepsBelow = 5;
    [Tooltip("Simulation sub-steps when the speed is below critical.")]
    public int stepsAbove = 1;

    [Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
    public DriveType driveType;

    public float SteerInput { get; set; }
    public float GasInput { get; set; }
    public float BrakeInput { get; set; }

    private WheelCollider[] wheels;
    private WheelCollider[] Wheels {
        get {
            if(wheels == null) {
                wheels = GetComponentsInChildren<WheelCollider>();
            }
            return wheels;
        }
    }

    // Find all the WheelColliders down in the hierarchy.
    void Start() {
        foreach(WheelCollider wheel in Wheels) {
            if(wheelShape != null) {
                var ws = Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }
    }

    public void Stop() {
        SteerInput = 0;
        GasInput = 0;
        BrakeInput = 0;
        SetWheelParam(0, 0, 0);
    }

    private void OnEnable() {
        foreach(var wheel in Wheels) {
            wheel.ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
        }
    }

    private void FixedUpdate() {
        float angle = maxAngle * SteerInput;
        float torque = maxTorque * GasInput;

        float handBrake = BrakeInput * brakeTorque;

        SetWheelParam(angle, torque, handBrake);
    }

    private void SetWheelParam(float angle, float torque, float brake) {
        foreach(WheelCollider wheel in Wheels) {
            // A simple car where front wheels steer while rear ones drive.
            if(wheel.transform.localPosition.z > 0) {
                wheel.steerAngle = angle;
            }

            if(wheel.transform.localPosition.z < 0) {
                wheel.brakeTorque = brake;
            }

            if(wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive) {
                wheel.motorTorque = torque;
            }

            if(wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive) {
                wheel.motorTorque = torque;
            }

            // Update visual wheels if any.
            if(wheelShape) {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // Assume that the only child of the wheelcollider is the wheel shape.
                Transform shapeTransform = wheel.transform.GetChild(0);

                if(wheel.name == "a0l" || wheel.name == "a1l" || wheel.name == "a2l") {
                    shapeTransform.rotation = q * Quaternion.Euler(0, 180, 0);
                    shapeTransform.position = p;
                }
                else {
                    shapeTransform.position = p;
                    shapeTransform.rotation = q;
                }
            }
        }
    }
}
