using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour
{
    public float spacing = 0.1f;
    public float resolution = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Vector2[] points = FindObjectOfType<PathCreator>().path.CalculateEvenlySpacedPoints(spacing, resolution);
        foreach(Vector2 p in points)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * spacing * 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
