using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalSensor : MonoBehaviour
{
    public abstract float[] ReadFromSensor();
    public abstract int NumOutputs();
}
