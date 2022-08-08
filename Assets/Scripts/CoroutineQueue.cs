using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoroutineQueue
{
    private struct QueueElement
    {
        public IEnumerator action;
        // public PassDelegate passCallback;
        // public FailDelegate failCallback;
        public string name;
        public QueueElement(IEnumerator action, /*PassDelegate passCallback, FailDelegate failCallback,*/ string name)
        {
            this.action = action;
            // this.passCallback = passCallback;
            // this.failCallback = failCallback;
            this.name = name;
        }
    }
    MonoBehaviour m_Owner = null;
    Coroutine m_InternalCoroutine = null;
    Queue<QueueElement> actions = new Queue<QueueElement>();

    private bool paused;
    public bool Paused   // property
    {
        get { return paused; }
        set { paused = value; }
    }

    public CoroutineQueue(MonoBehaviour aCoroutineOwner)
    {
        m_Owner = aCoroutineOwner;
    }
    public void StartLoop()
    {
        m_InternalCoroutine = m_Owner.StartCoroutine(Process());
    }
    public void StopLoop()
    {
        if (m_InternalCoroutine != null) { 
            m_Owner.StopCoroutine(m_InternalCoroutine);
            m_InternalCoroutine = null;
        }
    }
    public void EnqueueAction(IEnumerator aAction, string name)
    {
        actions.Enqueue(new QueueElement(aAction, name));
    }

    public void Clear()
    {
        actions.Clear();
    }

    private IEnumerator Process()
    {
        while (true)
        {
            if (actions.Count > 0 && !paused)
            {
                QueueElement action = actions.Dequeue();
                CoroutineWithData cd = new CoroutineWithData(m_Owner, action.action);
                yield return cd.coroutine;
                if (cd.result is CoroutineQueueResult)
                {
                    switch ((CoroutineQueueResult)cd.result)
                    {
                        case CoroutineQueueResult.PASS:
                            break;
                        case CoroutineQueueResult.FAIL:
                            paused = true;
                            break;
                    }
                }
                else
                {
                    // (m_Owner, "." + actionName + " Unclassified result is: " + cd.result);
                }
            }
            else
                yield return null;
        }
    }

    public void EnqueueWait(float aWaitTime)
    {
        actions.Enqueue(new QueueElement(Wait(aWaitTime), /*null, null,*/ "Wait " + Time.time));
    }

    private IEnumerator Wait(float aWaitTime)
    {
        yield return new WaitForSeconds(aWaitTime);
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}

public enum CoroutineQueueResult
{
    PASS, FAIL
}

public class Error
{
    public string cause;
}

public class Result
{
    public string message;
}

public delegate void PassDelegate(Result result);
public delegate void FailDelegate(Error error);

