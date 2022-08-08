using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[RequireComponent(typeof(AIController))]
public class BezierPathFitness : MonoBehaviour
{
    [SerializeField]
    float fitnessMultiplier;
    [SerializeField]
    float rewardInterval;

    AIController controller;
    PathCreator pathCreator;

    float pathTravelDir;
    float prevTimeOnPath;
    float currentTimeOnPath;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AIController>();    
        pathCreator = GameObject.FindObjectOfType<PathCreator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.IsUsingBrain && pathCreator != null && time >= rewardInterval)
        {
            time = 0;
            currentTimeOnPath = pathCreator.path.GetClosestTimeOnPath(transform.position);
            pathTravelDir = currentTimeOnPath - prevTimeOnPath;
            controller.AddBrainFitness = fitnessMultiplier * pathTravelDir * Time.deltaTime;
            prevTimeOnPath = currentTimeOnPath;
        }
        else
        {
            time += Time.deltaTime;
        }
    }
}
