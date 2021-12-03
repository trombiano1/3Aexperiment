// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool IsDone { get; private set; }
    public bool IsFrozen { get; private set; }

    public float Reward { get; private set; }

    public void SetReward(float reward) {
        Reward = reward;
    }

    public void AddReward(float reward) {
        Reward += reward;
    }

    public abstract int GetState();

    public abstract List<double> CollectObservations();

    public abstract void AgentAction(double[] vectorAction);

    public abstract void AgentReset();

    public abstract void Stop();

    public abstract double[] ActionNumberToVectorAction(int ActionNumber);

    public void Done()
    {
        IsDone = true;
    }

    public void Reset()
    {
        Stop();
        AgentReset();
        IsDone = false;
        IsFrozen = false;
        SetReward(0);
    }

}
