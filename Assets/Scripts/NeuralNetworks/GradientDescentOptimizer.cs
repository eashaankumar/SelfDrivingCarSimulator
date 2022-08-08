using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientDescentOptimizer : MonoBehaviour
{
    NeuralNetwork network;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Experiment());

    }

    IEnumerator Experiment()
    {
        network = new NeuralNetwork(new int[] { 2, 2, 1 });
        NeuralNetwork bestNetwork = network;
        float bestError = float.MaxValue;
        int epochs = 1000;
        while (epochs > 0)
        {
            network.Train(new float[] { 1, 0 }, new float[] { 1 }, 0.1f);
            yield return null;
            network.Train(new float[] { 0, 1 }, new float[] { 1 }, 0.1f);
            yield return null;
            network.Train(new float[] { 1, 1 }, new float[] { 0 }, 0.1f);
            yield return null;
            network.Train(new float[] { 0, 0 }, new float[] { 0 }, 0.001f);
            yield return null;
            if (network.ForwardPassError < bestError)
            {
                bestNetwork = new NeuralNetwork(network);
            }
            Debug.Log(network.ForwardPassError);
            epochs--;
        }
        Debug.Log(bestNetwork.ForwardPassError);
        Debug.Log(bestNetwork.FeedForward(new float[] { 1, 0 })[0]);
        Debug.Log(bestNetwork.FeedForward(new float[] { 0, 1 })[0]);
        Debug.Log(bestNetwork.FeedForward(new float[] { 1, 1 })[0]);
        Debug.Log(bestNetwork.FeedForward(new float[] { 0, 0 })[0]);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
