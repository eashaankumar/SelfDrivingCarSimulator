using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIController))]
public class RoadFitness : MonoBehaviour
{
    [SerializeField]
    int roadLayer;
    [SerializeField]
    float fitnessForBeingOnRoad;

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
                if (wheel.CurrentlyHittingCollider != null && wheel.CurrentlyHittingCollider.gameObject.layer == roadLayer)
                {
                    controller.AddBrainFitness = fitnessForBeingOnRoad * Time.fixedDeltaTime;
                }
            }
        }
    }
}
