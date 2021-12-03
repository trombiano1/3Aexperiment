// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RayPerception))]
public class Sensor : MonoBehaviour
{
    [Header("Settings"), SerializeField] private float distance = 0.0f;
    public float Distance { get { return distance; } }

    [SerializeField] private string layerName = string.Empty;
    public int Layer { get { return LayerMask.NameToLayer(layerName); } }

    [SerializeField] private float[] angles = null;
    public float[] Angles { get { return angles; } }

    [SerializeField] private RayPerception.CastType cast = RayPerception.CastType.Ray;
    private RayPerception.CastType Cast { get { return cast; } }

    [SerializeField] private bool isNormalized = false;
    private bool IsNormalized { get { return isNormalized; } }

    private RayPerception Perception { get; set; }

    private void Awake() {
        Perception = GetComponent<RayPerception>();
    }

    public List<double> Hits() {
        return Perception.Perceive(Distance, Angles, Layer, IsNormalized, Cast);
    }

    public List<Vector3> HitsPosition() {
        return Perception.PerceivePosition(Distance, Angles, Layer, Cast);
    }
}
