// SerialID: [d9c2059b-6dd6-4071-8f77-d635fae0e462]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class obstacle_stage3 : MonoBehaviour
{
    [SerializeField] private TransformEvent onEnter = null;
    private TransformEvent OnEnter => onEnter;


    private bool First = true;

    public void OnTriggerEnter(Collider other)
    {
        
        if (First)
        {
            OnEnter.Invoke(other.transform);
            First = false;
        }

    }

    [Serializable]
    private class TransformEvent : UnityEvent<Transform>
    {
    }
}
