using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpoitAlignment : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.yellow);
    }
}
