using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowCar : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    Vector3 positionOffset;

    [SerializeField]
    Vector3 lookAtOffset;

    [SerializeField]
    float positionDamping;
    [SerializeField]
    float upOrientationDamping;

    Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, target.position + target.forward * positionOffset.z + target.up * positionOffset.y + target.right * positionOffset.x, positionDamping);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.forward, out hit, 1f))
        {
            newPos.y = Mathf.Lerp(newPos.y, Mathf.Max(hit.point.y + 2f, newPos.y), 0.1f);
        }
        transform.position = newPos;
        //mainCam.transform.LookAt(lookAtOffset + target.position);
        Vector3 targetPos = mainCam.WorldToScreenPoint(target.position + lookAtOffset);
        Vector3 distanceFromCenterOfScreen = targetPos - new Vector3(Screen.width/2, Screen.height/2, 0);
        mainCam.transform.forward = Vector3.Lerp(mainCam.transform.forward, target.forward, Mathf.Abs(distanceFromCenterOfScreen.x) / Screen.width);
        //mainCam.transform.up = Vector3.Lerp(mainCam.transform.up, target.up, upOrientationDamping);
    }
}
