// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentExecutor : MonoBehaviour
{
    public enum BrainType
    {
        None = 0,
        Q,
        NE
    }

    [SerializeField] private bool resetOnDone = false;
    private bool ResetOnDone { get { return resetOnDone; } }

    [SerializeField] private List<AgentItem> agents = null;
    private List<AgentItem> Agents => agents;

    private void Start() {
        if(Agents == null || Agents.Count == 0) {
            return;
        }

        Agents.ForEach(item => {
            if(item.brainType == BrainType.Q) {
                item.brain = QBrain.Load(item.learning);
                item.run = RunQ;
            }
            else if(item.brainType == BrainType.NE) {
                item.brain = NNBrain.Load(item.learning);
                item.run = RunNE;
            }
        });
    }

    private void FixedUpdate() {
        Agents.ForEach(item => {
            if(item.agent.IsDone) {
                if(ResetOnDone) {
                    item.agent.Reset();
                }
                return;
            }

            item.run(item);
        });
    }

    private void RunQ(AgentItem item) {
        var observation = item.agent.GetState();
        var action = item.agent.ActionNumberToVectorAction(observation);
        item.agent.AgentAction(action);
    }

    private void RunNE(AgentItem item) {
        var observation = item.agent.CollectObservations();
        item.agent.AgentAction((item.brain as NNBrain).GetAction(observation));
    }

    [Serializable]
    public class AgentItem
    {
        public Brain brain;
        public Agent agent;
        public TextAsset learning;
        public BrainType brainType = BrainType.None;
        public Action<AgentItem> run;
    }
}
