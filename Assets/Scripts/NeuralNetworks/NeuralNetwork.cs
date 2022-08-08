using System.Collections.Generic;
using System;
using UnityEngine;

// Credit: https://www.youtube.com/watch?v=Yq0SfuiOVYE
public class NeuralNetwork: IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;
    private float[] biases;
    private float[] biasWeights;
    private float[][] neuronErrors;

    private float fitness;

    public float Fitness
    {
        get { return fitness; }
    }

    public int GetInputLayerSize
    {
        get { return layers[0]; }
    }

    public float ForwardPassError
    {
        get
        {
            if (neuronErrors == null) return float.MaxValue;
            float sum = 0;
            for(int errorI = 0; errorI < neuronErrors[neuronErrors.Length - 1].Length; errorI++)
            {
                sum += neuronErrors[neuronErrors.Length - 1][errorI];
            }
            return sum;
        }
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
        List<float[]> neuronsErrorLst = new List<float[]>();
        List<float> bias = new List<float>();
        for(int i = 0; i < layers.Length; i++)
        {
            neuronsLst.Add(new float[layers[i]]);
            neuronsErrorLst.Add(new float[layers[i]]);
            bias.Add(UnityEngine.Random.Range(0f, 1f));
        }
        neurons = neuronsLst.ToArray(); // jagged array
        neuronErrors = neuronsErrorLst.ToArray();
        biases = bias.ToArray();
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

        return neurons[neurons.Length-1]; // Redundant
    }

    private void NeuronActivations(int layerIndex)
    {
        for (int j = 0; j < neurons[layerIndex].Length; j++)
        {
            float activation = NeuronActivation(layerIndex, j);
            neurons[layerIndex][j] = TanhActivation(activation);
        }
    }

    private float TanhActivation(float activation)
    {
        return Mathf.Atan(activation);
    }

    private float TanhDerivative(float activation)
    {
        // activation = TanhActivation(neuron) so no need to repeat
        return 1 - Mathf.Pow(activation, 2);
    }

    /// <summary>
    /// Returns weight associated between two neurons
    /// </summary>
    /// <param name="fromNeuron">neuron index of previous layer</param>
    /// <param name="toNeuron">neuron index the weight adds to</param>
    /// <param name="tolayer">layer index of toNeuron</param>
    /// <returns>weight of fromNeuron->toNeuron</returns>
    public float NeuronWeight(int fromNeuron, int toNeuron, int tolayer)
    {
        return weights[tolayer - 1][toNeuron][fromNeuron];
    }

    /// <summary>
    /// Returns weight associated between two neurons
    /// </summary>
    /// <param name="fromNeuron">neuron index of previous layer</param>
    /// <param name="toNeuron">neuron index the weight adds to</param>
    /// <param name="tolayer">layer index of toNeuron</param>
    /// <param name="newWeight">new weight</param>
    /// <returns>weight of fromNeuron->toNeuron</returns>
    private void AddNeuronWeight(int fromNeuron, int toNeuron, int tolayer, float newWeight)
    {
        weights[tolayer - 1][toNeuron][fromNeuron] += newWeight;
    }

    private float NeuronActivation(int layerIndex, int neuronIndex)
    {
        float value = biases[layerIndex];
        for (int k = 0; k < neurons[layerIndex - 1].Length; k++)
        {
            value += NeuronWeight(k, neuronIndex, layerIndex) * neurons[layerIndex - 1][k];
        }
        return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputs">Actual values</param>
    /// <returns></returns>
    public void CalculateError(float[] actual, int layer, float[] errors)
    {
        Debug.Assert(actual.Length == neurons[layer].Length);
        for(int i = 0; i < actual.Length; i++)
        {
            errors[i] = actual[i] - neurons[layer][i];
        }
    }

    public void NeuronErrors(float[] actual)
    {
        CalculateError(actual, neurons.Length - 1, neuronErrors[neuronErrors.Length-1]);
        for(int layer = layers.Length - 2; layer >= 1; layer--)
        {
            for(int neuron = 0; neuron < neurons[layer].Length; neuron++)
            {
                for(int k = 0; k < neurons[layer+1].Length; k++)
                {
                    // weights[layer][neuron][k]
                    neuronErrors[layer][neuron] += NeuronWeight(neuron, k, layer+1) * neurons[layer+1][k];
                }
            }
        }
    }

    public void BackPropagate(float learningRate)
    {
        for(int layer = layers.Length - 1; layer >= 1; layer--) // go up to 1 to ignore input layer
        {
            float deltaBias = 0;
            for(int neuron = 0; neuron < neurons[layer].Length; neuron++)
            {
                float gradient = TanhDerivative(neurons[layer][neuron]) * neuronErrors[layer][neuron] * learningRate;
                deltaBias += gradient;
                for (int col = 0; col < neurons[layer - 1].Length; col++)
                {
                    float deltaW = gradient * neurons[layer - 1][col];
                    AddNeuronWeight(col, neuron, layer, deltaW);
                }
            }
            biases[layer] += deltaBias;
        }
    }

    /// <summary>
    /// Performs one iteration over one input and output pair
    /// </summary>
    /// <param name="input">One input instance</param>
    /// <param name="actual">Expected output</param>
    public void Train(float[] input, float[] actual, float learningRate)
    {
        Debug.Assert(input.Length == layers[0]);
        Debug.Assert(actual.Length == layers[layers.Length-1]);
        FeedForward(input);
        NeuronErrors(actual);
        BackPropagate(learningRate);
    }

    void PrintArray(float[] a)
    {
        string s = "";
        for(int i = 0; i < a.Length; i++)
        {
            s += a[i] + ", ";
        }
        Debug.Log(s);
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
