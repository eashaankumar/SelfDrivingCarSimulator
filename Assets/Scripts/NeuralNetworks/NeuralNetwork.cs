using System.Collections.Generic;
using System;
using UnityEngine;

public class NeuralNetwork: IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;

    private float fitness;

    public float Fitness
    {
        get { return fitness; }
    }


    /// <summary>
    /// layers = [input, hidden1, ..., output]
    /// </summary>
    /// <param name="layers"></param>
    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for(int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        InitNeurons();
        InitWeights();
    }

    /// <summary>
    /// Deep copy
    /// </summary>
    /// <param name="copyNetwork">Network to deep copy</param>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for(int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }
        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);
    }

    private void CopyWeights(float[][][] copyWeights)
    {
        for(int i = 0; i < copyWeights.Length; i++)
        {
            for(int j = 0; j < copyWeights[i].Length; j++)
            {
                for(int k = 0; k < copyWeights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    private void InitNeurons()
    {
        List<float[]> neuronsLst = new List<float[]>();
        for(int i = 0; i < layers.Length; i++)
        {
            neuronsLst.Add(new float[layers[i]]);
        }
        neurons = neuronsLst.ToArray(); // jagged array
    }

    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for(int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeight = new float[neuronsInPreviousLayer];
                for(int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeight[k] = UnityEngine.Random.Range(-0.5f,0.5f);
                }
                layerWeightList.Add(neuronWeight);
            }
            weightsList.Add(layerWeightList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)
    {
        for(int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for(int i = 1; i < layers.Length; i++)
        {
            NeuronActivations(i);
        }

        return neurons[neurons.Length-1];
    }

    private void NeuronActivations(int layerIndex)
    {
        for (int j = 0; j < neurons[layerIndex].Length; j++)
        {
            float activation = NeuronActivation(layerIndex, j, 0.25f);
            neurons[layerIndex][j] = TanhActivation(activation);
        }
    }

    private float TanhActivation(float activation)
    {
        return Mathf.Atan(activation);
    }

    private float TanhDerivative(float activation)
    {
        return 1 - Mathf.Pow(TanhActivation(activation), 2);
    }

    private float NeuronActivation(int layerIndex, int neuronIndex, float bias)
    {
        float value = 0.25f;
        for (int k = 0; k < neurons[layerIndex - 1].Length; k++)
        {
            value += weights[layerIndex - 1][neuronIndex][k] * neurons[layerIndex - 1][k];
        }
        return value;
    }

    public float CalculateError(float[] outputs)
    {
        float error = 0.0f;
        Debug.Assert(outputs.Length == neurons[neurons.Length-1].Length);
        for(int i = 0; i < outputs.Length; i++)
        {
            error += Mathf.Pow(outputs[i] - neurons[neurons.Length-1][i], 2);
        }
        return Mathf.Sqrt(error);
    }

    public void BackPropagate(float error, float stepSize)
    {

    }

    public void Mutate()
    {
        for(int i = 0; i < weights.Length; i++)
        {
            for(int j = 0; j < weights[i].Length; j++)
            {
                for(int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];
                    // mutate weight
                    float randomNumber = UnityEngine.Random.Range(0, 1000f);
                    if (randomNumber <= 2f)
                    {
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    {
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 6f)
                    {
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    {
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight += factor;
                    }
                    weights[i][j][k] = weight;
                }
            }
        }
    }

    public void AddFitness(float fit)
    {
        fitness += fit;
    }

    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;
        if (fitness > other.fitness) return 1;
        else if (fitness < other.fitness) return -1;
        else return 0;
    }

}
