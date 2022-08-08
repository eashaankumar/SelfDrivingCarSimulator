using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleControllerWheelRaycast))]
public class UserController : MonoBehaviour
{
    VehicleControllerWheelRaycast controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<VehicleControllerWheelRaycast>();    
    }

    // Update is called once per frame
    void Update()
    {
        controller.SteeringInput = Input.GetAxis("Horizontal");
        controller.GasInput = Input.GetAxis("Vertical");
        controller.BrakeInput = Input.GetKey(KeyCode.Space);
    }
}
