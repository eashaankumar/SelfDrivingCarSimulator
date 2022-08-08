using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{

    [SerializeField]
    float mouseSensitivity;
    [SerializeField]
    float speed;

    float forward;
    float right;
    Vector3 dirMove;
    float mouseX;
    float mouseY;
    float xRot = 0;
    float yRot = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        yRot += mouseX;
        transform.localRotation = Quaternion.Euler(xRot, yRot, 0);

        forward = Input.GetAxis("Vertical");
        right = Input.GetAxis("Horizontal");
        dirMove = transform.forward * forward + transform.right * right;
        dirMove.Normalize();
        transform.position += dirMove * speed * Time.deltaTime;
    }
}
