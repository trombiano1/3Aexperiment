// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class TriggerObserver : MonoBehaviour
{
    [SerializeField] private TransformEvent onEnter = new TransformEvent();
    public TransformEvent OnEnter => onEnter;

    [SerializeField] private Obstacle targetObstacle = null;
    public Obstacle TargetObstacle { get { return targetObstacle; } set { targetObstacle = value; } }

    public void OnTriggerEnter(Collider other) {
        OnEnter.Invoke(other.transform);
        if(TargetObstacle != null) {
            TargetObstacle.SetForce(other.transform);
        }
    }

    [Serializable] public class TransformEvent : UnityEvent<Transform> {
    }
}
