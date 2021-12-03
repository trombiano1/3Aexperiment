// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class QEnvironment : Environment
{
    [SerializeField] private GameObject gObject = null;
    private GameObject GObject => gObject;

    [SerializeField] private int actionSize = 6;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private int stateSize = 4;
    private int StateSize { get { return stateSize; } }

    [Header("UI References"), SerializeField] private Text episodeText = null;
    private Text EpisodeText { get { return episodeText; } }

    private QBrain QLBrain { get; set; }
    private Agent LearningAgent { get; set; }

    private int PrevState { get; set; }
    private int EpisodeCount { get; set; }

    void Start() {
        QLBrain = new QBrain(StateSize, ActionSize);
        LearningAgent = GObject.GetComponent<Agent>();
        PrevState = LearningAgent.GetState();
        UpdateText();
    }

    private void FixedUpdate() {
        AgentUpdate(LearningAgent, QLBrain);
        if(LearningAgent.IsDone) {
#if UNITY_EDITOR
            var path = string.Format("Assets/LearningData/Q/{0}.bytes", EditorSceneManager.GetActiveScene().name);
            QLBrain.Save(path);
#endif
            EpisodeCount++;

            LearningAgent.Reset();
            UpdateText();
        }
    }

    private void AgentUpdate(Agent a, QBrain b) {
        int actionNo = b.GetAction(PrevState);
        var action = a.ActionNumberToVectorAction(actionNo);
        a.AgentAction(action);
        var newState = a.GetState();
        b.UpdateTable(PrevState, newState, actionNo, a.Reward, a.IsDone);
        a.SetReward(0);
        PrevState = newState;
    }

    private void UpdateText() {
        EpisodeText.text = "Episode: " + EpisodeCount;
    }
}
