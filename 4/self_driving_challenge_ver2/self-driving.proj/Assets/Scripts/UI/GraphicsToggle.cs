// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GraphicsToggle : MonoBehaviour
{
    private List<MeshRenderer> Renderer { get; set; } = new List<MeshRenderer>();

    public void OnToggle(bool b) {
        if(b) {
            Renderer.ForEach(r => r.enabled = true);
        }
        else {
            Renderer.Clear();
            Renderer.AddRange(FindObjectsOfType<MeshRenderer>());
            Renderer.ForEach(r => r.enabled = false);
        }
    }
}
