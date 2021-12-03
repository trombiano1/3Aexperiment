// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public void OnTimeScaleChanged(float timeScale) {
        Time.timeScale = timeScale;
    }
}
