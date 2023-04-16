using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.ReplayableEffects;
using UnityEngine;

[RequireComponent(typeof(Zone))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private EffectPool m_collect;
    public enum PickupType
    {
        Needle,
        Thread
    }

    [SerializeField]
    private PickupType m_type;

    private bool m_collected;
    private Zone m_zone;
    private void Awake()
    {
        m_zone = GetComponent<Zone>();
        m_zone.ActorEnterExit += OnActorEnterExit;
    }

    private void OnDestroy()
    {
        if(m_zone)
            m_zone.ActorEnterExit -= OnActorEnterExit;
    }

    private void OnActorEnterExit(Actor actor, bool enter)
    {
        if(m_collected || actor is not Avatar avatar)
            return;
        
        switch (m_type)
        {
            case PickupType.Needle:
                if(avatar.HasNeedle)
                    return;
                avatar.HasNeedle = true;
                break;
                
            case PickupType.Thread:
                if(avatar.HasThread)
                    return;
                avatar.HasThread = true;
                break;
        }

        m_collected = true;
        

        PlayCollectEffect();
    }
    
    private void PlayCollectEffect()
    {
        if(m_collect)
            m_collect.PlayEffect(transform);
        Destroy(gameObject);
    }
}
