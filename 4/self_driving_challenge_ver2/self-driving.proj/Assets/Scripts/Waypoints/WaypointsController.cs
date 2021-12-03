// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaypointsController : MonoBehaviour
{
    [Header("Waypoint Settings"), SerializeField, Range(1, 40)] private float distance = 1;
    public float Distance => distance;

    [SerializeField] private float height = 10.0f;
    public float Height => height;

    [SerializeField] private int size = 100;
    public int Size => size;

    [SerializeField] private Waypoint prefab = null;
    public Waypoint Prefab => prefab;

    [Header("Obstacle Settings"), SerializeField] private int obstacleRate = 10;
    public int ObstacleRate => obstacleRate;

    [SerializeField] private int obstacleFirePosition = -8;
    public int ObstacleFirePosition => obstacleFirePosition;

    [SerializeField] private Obstacle obstaclePrefab = null;
    public Obstacle ObstaclePrefab => obstaclePrefab;

    [SerializeField] private float minForce = 100.0f;
    public float MinForce => minForce;

    [SerializeField] private float maxForce = 300.0f;
    public float MaxForce => maxForce;

    [SerializeField] private float minScale = 0.5f;
    public float MinScale { get { return minScale; } set { minScale = value; } }

    [SerializeField] private float maxScale = 1.5f;
    public float MaxScale { get { return maxScale; } set { maxScale = value; } }
}
