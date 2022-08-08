using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionaryOptimizer : MonoBehaviour
{
    [SerializeField]
    int samples;
    [SerializeField]
    AIController controllerPrefab;
    [SerializeField]
    Transform spawnPoint;
    [SerializeField, Tooltip("Time (in seconds) since round start when evolution kicks in")]
    float roundDuration;
    [SerializeField, Tooltip("Ignores the checked layers when training (for avoiding collisions with other training controllers)")]
    int trainingCollisionLayer;
    [SerializeField]
    Button startRoundButton;
    [SerializeField]
    bool loopTraining;

    CoroutineQueue actions;

    public delegate void OptimizerEvent();
    public delegate void OptimizerEventWithBrain(NeuralNetwork brain);
    public event OptimizerEventWithBrain RoundStart;
    public event OptimizerEvent RoundEnd;
    NeuralNetwork bestBrain;
    AIController[] controllers;

    // Start is called before the first frame update
    void Start()
    {
        actions = new CoroutineQueue(this);
        actions.StartLoop();
        actions.EnqueueAction(InitControllers(), "Init Controllers");
    }

    public void StartRound()
    {
        actions.EnqueueAction(NewRound(), "New Round");
    }

    IEnumerator InitControllers()
    {
        startRoundButton.interactable = false;
        controllers = new AIController[samples];
        for (int i = 0; i < samples; i++)
        {
            controllers[i] = Instantiate(controllerPrefab);
            controllers[i].gameObject.GetComponentInChildren<MeshCollider>().gameObject.layer = trainingCollisionLayer;
            controllers[i].gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
        }
        startRoundButton.interactable = true;
        yield return CoroutineQueueResult.PASS;
        yield break;
    }

    IEnumerator NewRound()
    {
        startRoundButton.interactable = false;
        
        for (int i = 0; i < samples; i++)
        {
            controllers[i].gameObject.transform.position = spawnPoint.position;
            controllers[i].gameObject.transform.forward = spawnPoint.forward;
            controllers[i].gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitUntil(() => RoundStart != null && RoundStart.GetInvocationList().Length >= samples);
        RoundStart(bestBrain);
        yield return new WaitWhile(() => !AllControllersActive(controllers));
        yield return new WaitForSeconds(roundDuration);
        float bestFitness = bestBrain != null ? bestBrain.Fitness : float.MinValue;
        int bestBrainIndex = -1;

        for (int i = 0; i < samples; i++)
        {
            if (controllers[i].CurrentFitness > bestFitness)
            {
                controllers[i].StopUsingBrain();
                bestFitness = controllers[i].CurrentFitness;
                bestBrainIndex = i;
            }
        }

        for (int i = 0; i < samples; i++)
        {
            controllers[i].gameObject.SetActive(false);
        }

        if (bestBrainIndex != -1)
        {
            bestBrain = controllers[bestBrainIndex].BrainDeepCopy;
        }

        if (RoundStart.GetInvocationList().Length > 0)
        {
            RoundEnd();
        }

        print("Best Fitness: " + bestFitness);

        startRoundButton.interactable = true;
        if (loopTraining)
        {
            StartRound();
        }
        yield return CoroutineQueueResult.PASS;
        yield break;
    }

    bool AllControllersActive(AIController[] controllers)
    {
        bool allControllersRunning = true;
        for (int i = 0; i < samples; i++)
        {
            allControllersRunning &= controllers[i].IsUsingBrain;
        }
        return allControllersRunning;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
