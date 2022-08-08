using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SheepController))]
public class Sheep : Animal
{

    SheepController controller;

    private float[] senseData;

    float[] outputs;

    protected new void Awake()
    {
        base.Awake();
        controller = GetComponent<SheepController>();
    }

    IEnumerator Behavior()
    {
        while (true)
        {
            Sense();
            yield return null;
            Plan();
            yield return null;
            Act();
            yield return new WaitUntil(() => controller.ActedOnStateChanged);
        }
    }

    protected override void InitializeBrain()
    {
        int inputNeurons = 0;
        foreach(AnimalSensor sensor in this.sensors)
        {
            inputNeurons += sensor.NumOutputs();
        }
        // rest, dir, step, eat, drink, play, reproduce
        int outputsNeurons = 1 + 3 + 1 + 1 + 1 + 1 + 1;
        int[] layers = new int[1 + this.brainHiddenLayers.Length + 1];
        layers[0] = inputNeurons;
        layers[layers.Length-1] = outputsNeurons;
        for(int i = 0; i < this.brainHiddenLayers.Length; i++)
        {
            layers[i+1] = this.brainHiddenLayers[i];
        }
        network = new NeuralNetwork(layers);
        actions.EnqueueAction(Behavior(), "Behavior");
    }

    protected override void Sense()
    {
        List<float> senseDataList = new List<float>();
        foreach (AnimalSensor sensor in this.sensors)
        {
            float[] data = sensor.ReadFromSensor();
            for (int i = 0; i < data.Length; i++)
            {
                senseDataList.Add(data[i]);
            }
        }
        senseData = senseDataList.ToArray();
    }

    protected override void Plan()
    {
        outputs = network.FeedForward(senseData);
    }

    protected override void Act()
    {
        bool rest = Mathf.RoundToInt(outputs[0]) == 1;
        Vector3 direction = new Vector3(outputs[1], outputs[2], outputs[3]);
        float step = Mathf.Clamp01(outputs[4]);
        bool eat = Mathf.RoundToInt(outputs[5]) == 1;
        bool drink = Mathf.RoundToInt(outputs[6]) == 1;
        bool play = Mathf.RoundToInt(outputs[7]) == 1;
        bool reproduce = Mathf.RoundToInt(outputs[8]) == 1;
        controller.SetControls(rest, direction, step, eat, drink, play, reproduce);
    }
}
