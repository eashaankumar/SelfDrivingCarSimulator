using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIController))]
public class SpeedFitness : MonoBehaviour
{
    [SerializeField, Tooltip("Speed to fitness conversion ratio")]
    float speedMultiplier;

    AIController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AIController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (controller.IsUsingBrain)
        {
            foreach (VehicleControllerWheelRaycast wheel in controller.Wheels)
            {
                if (wheel.IsGrounded)
                {
                    controller.AddBrainFitness = wheel.TireWorldVel.sqrMagnitude * speedMultiplier * Time.fixedDeltaTime;
                }

            }
        }
    }
}
