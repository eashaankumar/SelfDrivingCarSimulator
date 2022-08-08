using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SubmarineController : MonoBehaviour
{
    [SerializeField]
    float forwardForce;
    [SerializeField]
    float turnForce;
    [SerializeField]
    Transform turnArm;

    Rigidbody rb;
    float verticalInput, horizontalInput;
    bool elevatorsOn;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        elevatorsOn = Input.GetMouseButton(0);
    }

    private void FixedUpdate()
    {
        Vector3 netforwardForce = transform.forward * forwardForce * verticalInput;
        Vector3 netTurnForce = (elevatorsOn ? transform.up : transform.right) * turnForce * horizontalInput;
        rb.AddForce(netforwardForce);
        rb.AddForceAtPosition(netTurnForce, turnArm.position);
    }
}
