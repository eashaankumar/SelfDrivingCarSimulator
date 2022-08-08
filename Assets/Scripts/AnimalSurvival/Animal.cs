using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Animal : MonoBehaviour
{
    [SerializeField]
    protected float movementMultiplier = 1f;
    [SerializeField]
    protected float staminaMultiplier = 1f;
    [SerializeField]
    protected float thirstMultiplier = 1f;
    [SerializeField]
    protected float hungerMultiplier = 1f;
    [SerializeField, Tooltip("Number of neurons in each hidden layer (exluding input and output layers)")]
    protected int[] brainHiddenLayers;

    protected AnimalSensor[] sensors;
    protected NeuralNetwork network;
    protected CoroutineQueue actions;

    protected void Awake()
    {
        sensors = GetComponents<AnimalSensor>();
        actions = new CoroutineQueue(this);
        actions.StartLoop();
        InitializeBrain();
    }

    protected abstract void InitializeBrain();
    protected abstract void Sense();
    protected abstract void Plan();
    protected abstract void Act();
}
