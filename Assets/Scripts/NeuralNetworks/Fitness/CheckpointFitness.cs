using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIController))]
public class CheckpointFitness : MonoBehaviour
{
    [SerializeField, Tooltip("A one time hit bonus")]
    float fitnessBonus;

    AIController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AIController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Checkpoint"))
        {
            if (controller.IsUsingBrain)
            {
                float alignment = Vector3.Dot(controller.transform.position, other.gameObject.transform.position);
                controller.AddBrainFitness = fitnessBonus * alignment;
            }
        }
    }
}
