using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Zone))]
public class Trigger : MonoBehaviour
{
    //public UnityEvent<bool> OnEnterExit = new();
    public UnityEvent OnEnter = new();
    public float m_delay;
    public bool m_once = true;
    private bool m_fired = false;
    
    protected void Awake()
    {
        GetComponent<Zone>().ActorEnterExit += OnActorEnterExit;
    }
    
    private void OnActorEnterExit(Actor arg1, bool enter)
    {
        if(arg1 is not Avatar avatar)
            return;

        if(m_once && m_fired)
            return;

        m_fired = true;
        Invoke(nameof(InvokeTrigger), m_delay);
    }
    
    private void InvokeTrigger()
    {
        
        OnEnter.Invoke();
    }
}
