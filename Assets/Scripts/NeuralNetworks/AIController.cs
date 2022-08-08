using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField]
    int[] hiddenLayers;
    [SerializeField]
    VehicleControllerWheelRaycast[] wheelControllers;

    NeuralNetwork brain;
    int inputParamsPerWheel;
    EvolutionaryOptimizer optimizer;
    bool isUsingBrain;
    public bool IsUsingBrain
    {
        get { return isUsingBrain; }
    }

    public VehicleControllerWheelRaycast[] Wheels
    {
        get { return wheelControllers; }
    }

    public float AddBrainFitness
    {
        set
        {
            if (brain != null)
            {
                brain.AddFitness(value);
            }
        }
    }

    /// <summary>
    /// Amount of reward currently possessed
    /// </summary>
    public float CurrentFitness
    {
        get { return (brain != null) ? brain.Fitness : 0; }
    }

    public NeuralNetwork BrainDeepCopy
    {
        get { return (brain != null) ? new NeuralNetwork(brain) : null; }
    }

    private void Start()
    {
        optimizer = GameObject.FindObjectOfType<EvolutionaryOptimizer>();
        if(optimizer != null)
        {
            optimizer.RoundStart += UseBrain;
            optimizer.RoundEnd += StopUsingBrain;
        }
    }

    private void OnDestroy()
    {
        if (optimizer != null)
        {
            optimizer.RoundStart -= UseBrain;
            optimizer.RoundEnd -= StopUsingBrain;
        }
    }

    // Start is called before the first frame update
    public void UseBrain(NeuralNetwork starterBrain)
    {
        StartCoroutine(RunBrain(starterBrain));
    }

    public void StopUsingBrain()
    {
        isUsingBrain = false;
    }

    IEnumerator RunBrain(NeuralNetwork starterBrain)
    {
        if (starterBrain == null)
        {
            int pos = 3, rot = 3, vel = 3, suspension = 3, engine = 3, steering = 3, wheelFriction = 3, brake = 3;
            int out_horizontal = 1, out_vertical = 1, space = 1;
            inputParamsPerWheel = pos + rot + vel + suspension + engine + steering + wheelFriction + brake;
            int outputParams = out_horizontal + out_vertical + space;
            int numWheels = wheelControllers.Length;
            int[] layers = new int[hiddenLayers.Length + 2]; // input and output
            for (int i = 1; i < layers.Length - 1; i++)
            {
                layers[i] = hiddenLayers[i - 1];
            }
            layers[0] = inputParamsPerWheel * numWheels;
            layers[layers.Length - 1] = outputParams;
            brain = new NeuralNetwork(layers);
        }
        else
        {
            brain = new NeuralNetwork(starterBrain);
            brain.Mutate();
        }
        yield return null;
        List<float> input = new List<float>();
        isUsingBrain = true;
        while (IsUsingBrain)
        {
            BrainForwardPass(input);
            yield return new WaitForSeconds(2);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void BrainForwardPass(List<float> input)
    {
        input.Clear();
        for (int wheelI = 0; wheelI < wheelControllers.Length; wheelI++)
        {
            VehicleControllerWheelRaycast wc = wheelControllers[wheelI];
            // position
            input.Add(wc.transform.position.x);
            input.Add(wc.transform.position.y);
            input.Add(wc.transform.position.z);
            // rotation
            input.Add(wc.transform.forward.x);
            input.Add(wc.transform.forward.y);
            input.Add(wc.transform.forward.z);
            // velocity
            input.Add(wc.TireWorldVel.x);
            input.Add(wc.TireWorldVel.y);
            input.Add(wc.TireWorldVel.z);
            // suspension
            input.Add(wc.NetSuspensionForce.x);
            input.Add(wc.NetSuspensionForce.y);
            input.Add(wc.NetSuspensionForce.z);
            // engine
            input.Add(wc.NetEngineForce.x);
            input.Add(wc.NetEngineForce.y);
            input.Add(wc.NetEngineForce.z);
            // steering
            input.Add(wc.NetSteeringForce.x);
            input.Add(wc.NetSteeringForce.y);
            input.Add(wc.NetSteeringForce.z);
            // wheel friction
            input.Add(wc.NetWheelFrictionForce.x);
            input.Add(wc.NetWheelFrictionForce.y);
            input.Add(wc.NetWheelFrictionForce.z);
            // brake
            input.Add(wc.NetBrakeForce.x);
            input.Add(wc.NetBrakeForce.y);
            input.Add(wc.NetBrakeForce.z);
        }
        float[] outputs = brain.FeedForward(input.ToArray());

        float horizontalInput = outputs[0];
        float verticalInput = outputs[1];
        float breakInput = outputs[2];
        foreach (VehicleControllerWheelRaycast wc in wheelControllers)
        {
            wc.SteeringInput = horizontalInput;
            wc.GasInput = verticalInput;
            wc.BrakeInput = Mathf.RoundToInt(breakInput) == 1;
        }
    }
}
