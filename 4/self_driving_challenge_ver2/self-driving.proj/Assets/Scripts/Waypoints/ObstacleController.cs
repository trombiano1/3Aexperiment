// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private List<Obstacle> Obstacles { get; } = new List<Obstacle>();

    private void Start() {
        Obstacles.AddRange(FindObjectsOfType<Obstacle>());
    }

    public void SetRandom() {
        Obstacles.ForEach(o => o.SetRandomPosition());
    }

    public void ResetPosition() {
        Obstacles.ForEach(o => o.ResetPosition());
    }
    public void SetForce(Transform target)
    {
        Obstacles.ForEach(o => o.SetForce(target));
    }

    public void ResetAndForce(Transform target)
    {
        ResetPosition();
        SetForce(target);
    }
    public void ResetRandomAndForce(Transform target)
    {
        Obstacles.ForEach(o => o.ResetRandomAndForce(target));
    }
}
