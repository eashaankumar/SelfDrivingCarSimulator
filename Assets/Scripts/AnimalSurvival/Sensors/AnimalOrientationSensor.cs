using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalOrientationSensor : AnimalSensor
{
    public override float[] ReadFromSensor()
    {
        return new float[] { transform.forward.x, transform.forward.y, transform.forward.z };
    }

    public override int NumOutputs()
    {
        return 3;
    }
}
