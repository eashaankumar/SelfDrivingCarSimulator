using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPositionSensor : AnimalSensor
{
    public override float[] ReadFromSensor()
    {
        return new float[] { transform.position.x, transform.position.y, transform.position.z };
    }

    public override int NumOutputs()
    {
        return 3;
    }
}
