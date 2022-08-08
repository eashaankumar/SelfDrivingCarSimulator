using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SheepController : MonoBehaviour
{
    [SerializeField]
    float currentStepSize;
    [SerializeField]
    float maxJumpHeight;
    [SerializeField]
    float jumpSpeed;
    [SerializeField]
    float turnSpeed;
    [SerializeField]
    LayerMask dontMoveMask;
    [SerializeField]
    TextMeshPro text;

    bool actedOnStateChanged; // acted upon new input
    State state;
    Vector3 targetDir;
    float targetStepRatio;

    CoroutineQueue actions;

    private void Awake()
    {

        actions = new CoroutineQueue(this);
        actions.StartLoop();
    }

    public bool ActedOnStateChanged
    {
        get { return actedOnStateChanged; }
    }
    public void SetControls(bool rest, Vector3 direction, float step, bool eat, bool drink, bool play, bool reproduce)
    {
        actedOnStateChanged = false;
        if (rest) state = State.REST;
        else if (eat) state = State.EAT;
        else if (drink) state = State.DRINK;
        else if (play) state = State.PLAY;
        else if (reproduce) state = State.REPRODUCE;
        else { 
            targetDir = direction.normalized;
            targetStepRatio = step;
            state = State.MOVE;
        }
        // TODO: Remove this
        // state = State.MOVE;

        text.text = state.ToString();
        actions.EnqueueAction(HandleState(), "HandleState");
    }

    Vector2 Lateral(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    IEnumerator HandleState()
    {
        if (!actedOnStateChanged)
        {
            switch (state)
            {
                case State.REST:
                    break;
                case State.EAT:
                    break;
                case State.MOVE:
                    RaycastHit hit;
                    if (!Physics.Raycast(transform.position, targetDir, out hit, targetStepRatio* currentStepSize, dontMoveMask.value))
                    {
                        Vector2 landingPosition = Lateral(transform.position + targetDir * targetStepRatio * currentStepSize);
                        // TODO: Remove this
                        // landingPosition = new Vector2(transform.position.x + Random.Range(-2f, 2f), transform.position.z + Random.Range(-2f, 2f));
                        targetDir = new Vector3(landingPosition.x, 0f, landingPosition.y) - transform.position;
                        actions.EnqueueAction(RotateToDir(targetDir), "Rotate to dir " + targetDir);
                        actions.EnqueueAction(JumpToPos(landingPosition), "Moving to position " + landingPosition);
                    }
                    break;
                case State.DRINK:
                    break;
                case State.PLAY:
                    break;
                case State.REPRODUCE:
                    break;
                default:
                    break;
            }
        }
        yield return CoroutineQueueResult.PASS;
        yield break;
    }

    IEnumerator RotateToDir(Vector3 targetDir)
    {
        Vector2 lateralDir = Lateral(targetDir).normalized;
        if (lateralDir.sqrMagnitude != 0f)
        {
            float time = 0;
            Vector3 startDir = transform.forward;
            float angle = Vector2.SignedAngle(lateralDir, Lateral(startDir).normalized);
            while (time <= 1f)
            {
                yield return new WaitForFixedUpdate();
                transform.forward = Quaternion.AngleAxis(Mathf.Lerp(0f, angle, time), Vector3.up) * startDir;
                time += Time.fixedDeltaTime * turnSpeed;
            }
            transform.forward = targetDir - Vector3.up * targetDir.y;
        }
        yield return CoroutineQueueResult.PASS;
        yield break;
    }

    IEnumerator JumpToPos(Vector2 landingPosition)
    {
        float time = 0;
        float distanceToJump = Vector3.Distance(landingPosition, Lateral(transform.position));
        if (distanceToJump > 0)
        {
            float porabolaOffset = maxJumpHeight;
            float porabolaScale = (4 * porabolaOffset) / Mathf.Pow(distanceToJump, 2);
            float targetY = 0.5f; // Get terrain height at landingPos
            float startY = transform.position.y;
            float startX = transform.position.x;
            float startZ = transform.position.z;
            print("Hi");
            while (time <= 1f)
            {
                yield return new WaitForFixedUpdate();
                float y = Mathf.Lerp(startY + maxJumpHeight, targetY, Mathf.InverseLerp(0.5f, 1f, time));
                if (time < 0.5f)
                {
                    y = Mathf.Lerp(startY, startY + maxJumpHeight, Mathf.InverseLerp(0f, 0.5f, time));
                }
                transform.position = new Vector3(Mathf.Lerp(startX, landingPosition.x, time),
                                                 y, //-porabolaScale * time * time + porabolaOffset + transform.position.y,
                                                 Mathf.Lerp(startZ, landingPosition.y, time));
                time += Time.fixedDeltaTime * jumpSpeed;
            }
            transform.position = new Vector3(landingPosition.x, targetY, landingPosition.y);
        }
        float jumpRecoverTime = 1.2f;
        yield return new WaitForSeconds(jumpRecoverTime);
        actedOnStateChanged = true;
        yield return CoroutineQueueResult.PASS;
        yield break;
    }

    enum State
    {
        REST, MOVE, EAT, DRINK, PLAY, REPRODUCE
    }
}
