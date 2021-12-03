// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class Brain
{
    public float Reward { get; set; }

    public abstract void Save(string path);
}
