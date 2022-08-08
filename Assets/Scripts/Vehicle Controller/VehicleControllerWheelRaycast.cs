using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=CdPYlj5uZeI
public class VehicleControllerWheelRaycast : MonoBehaviour
{    
    [SerializeField]
    Rigidbody carRigidBody;
    [SerializeField]
    Transform carTransform;
    [SerializeField, Tooltip("Layers the wheel will collide with")]
    LayerMask layersForCollision;

    [Header("Suspension")]
    [SerializeField]
    float wheelRadius;
    [SerializeField]
    float suspensionRestDist;
    [SerializeField]
    float springStrength;
    [SerializeField]
    float springDamper;

    [Header("Steering and Grip")]
    [SerializeField, Tooltip("0=no grip, 1=full grip")]
    AnimationCurve tireGripFactor;
    [SerializeField]
    float tireMass;
    [SerializeField]
    float maxSteeringAngle;
    [SerializeField]
    bool isSteerable;
    [SerializeField]
    Transform tireTransform;
    [SerializeField]
    Transform tireGraphics;

    [Header("Engine")]
    [SerializeField]
    AnimationCurve enginePower;
    [SerializeField]
    float carTopSpeed;
    [SerializeField]
    float maxEngineTorque;

    [Header("Wheel Friction")]
    [SerializeField]
    float wheelFriction;

    [Header("Braking")]
    [SerializeField, Tooltip("0=no brake, 1=full brake"), Range(0f,1f)]
    float brakeStrength;

    bool breakInput;
    float steeringInput;
    float accelInput;
    float wheelDistancePerRev, wheelAngularVel;
    Vector3 tireWorldVel;
    Vector3 netSuspensionForce;
    Vector3 netEngineForce;
    Vector3 netSteeringForce;
    Vector3 netWheelFrictionForce;
    Vector3 netBrakeForce;
    Collider currentlyHitting;

    #region outputs
    public bool IsGrounded
    {
        get
        {
            return currentlyHitting != null;
        }
    }
    public Collider CurrentlyHittingCollider
    {
        get { return currentlyHitting; }
    }
    public Vector3 TireWorldVel
    {
        get { return tireWorldVel; }
    }
    public Vector3 NetSuspensionForce
    {
        get { return netSuspensionForce; }
    }
    public Vector3 NetEngineForce
    {
        get { return netEngineForce; }
    }
    public Vector3 NetSteeringForce
    {
        get { return netSteeringForce; }
    }
    public Vector3 NetWheelFrictionForce
    {
        get { return netWheelFrictionForce; }
    }
    public Vector3 NetBrakeForce
    {
        get { return netBrakeForce; }
    }
    #endregion

    #region inputs
    public float SteeringInput
    {
        set { steeringInput = value; }
    }
    public float GasInput
    {
        set { accelInput = value; }
    }
    public bool BrakeInput
    {
        set { breakInput = value; }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isSteerable)
        {
            tireTransform.right = Quaternion.AngleAxis(steeringInput * maxSteeringAngle, carTransform.up) * carTransform.right;
        }
       
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        currentlyHitting = null;
        Physics.Raycast(transform.position, -transform.up, out hit, suspensionRestDist + wheelRadius, layersForCollision.value);
        if (hit.collider != null) {
            currentlyHitting = hit.collider;
            // Suspension
            Debug.DrawLine(transform.position, transform.position - transform.up * (hit.distance), Color.yellow);
            Vector3 springDir = tireTransform.up;
            tireWorldVel = carRigidBody.GetPointVelocity(tireTransform.position);
            float offset = suspensionRestDist - hit.distance;
            Debug.DrawLine(transform.position, transform.position - transform.up * (offset), Color.red);
            float vel = Vector3.Dot(springDir, tireWorldVel);
            float force = (offset * springStrength) - (vel * springDamper);
            netSuspensionForce = springDir * force;
            carRigidBody.AddForceAtPosition(netSuspensionForce, tireTransform.position);
            tireTransform.position = transform.position + (-hit.distance + wheelRadius) * transform.up;


            // Steering
            Debug.DrawRay(tireTransform.position, tireTransform.forward, Color.blue);
            Debug.DrawRay(tireTransform.position, tireTransform.right, Color.red);
            Vector3 steeringDir = tireTransform.right;
            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            float desiredVelChange = -steeringVel * tireGripFactor.Evaluate(steeringVel);
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            netSteeringForce = steeringDir * tireMass * desiredAccel;
            carRigidBody.AddForceAtPosition(netSteeringForce, tireTransform.position);

            // Engine
            Vector3 accelDir = tireTransform.forward;
            float carSpeed = Vector3.Dot(carTransform.forward, carRigidBody.velocity);
            if (Mathf.Abs(accelInput) > 0f)
            {
                // forward speed of car in direction of driving
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
                float availableTorque = enginePower.Evaluate(normalizedSpeed) * accelInput * maxEngineTorque;
                netEngineForce = accelDir * availableTorque;
                carRigidBody.AddForceAtPosition(netEngineForce, tireTransform.position);
            }
            else
            {
                netEngineForce = Vector3.zero;
            }
            wheelDistancePerRev = 180f * wheelRadius;
            wheelAngularVel = tireWorldVel.magnitude * (1 / wheelDistancePerRev);
            tireGraphics.Rotate(Vector3.right * transform.localScale.x * carSpeed, wheelAngularVel * 360f, Space.Self);

            // Wheel friction
            Vector3 wheelFrictionDir = tireTransform.forward;
            float wheelFrictionVel = Vector3.Dot(wheelFrictionDir, tireWorldVel);
            float desiredFrictionVelChange = -wheelFrictionVel * wheelFriction;
            float desiredWheelFrictionAccel = desiredFrictionVelChange / Time.fixedDeltaTime;
            netWheelFrictionForce = wheelFrictionDir * tireMass * desiredWheelFrictionAccel;
            carRigidBody.AddForceAtPosition(netWheelFrictionForce, tireTransform.position);

            // Brakes
            Vector3 breakingDir = tireTransform.forward;
            float breakingVel = Vector3.Dot(breakingDir, tireWorldVel);
            float desiredBrakeVelChange = -breakingVel * brakeStrength;
            float desiredBrakeAccel = desiredBrakeVelChange / Time.fixedDeltaTime;
            netBrakeForce = breakingDir * tireMass * desiredBrakeAccel;
            carRigidBody.AddForceAtPosition(netBrakeForce, tireTransform.position);
        }
        else
        {
            netSuspensionForce = Vector3.zero;
            netSteeringForce = Vector3.zero;
            netEngineForce = Vector3.zero;
            netWheelFrictionForce = Vector3.zero;
            netBrakeForce = Vector3.zero;
        }
    }
}
