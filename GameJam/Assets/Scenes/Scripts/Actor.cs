using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents dynamic world object
/// </summary>
[DisallowMultipleComponent]
public class Actor : MonoBehaviour
{
    public event Action<Actor> Destroyed;
    
    protected virtual void OnDestroy()
    {
        Destroyed?.Invoke(this);   
    }
}
