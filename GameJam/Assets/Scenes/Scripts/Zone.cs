using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class Zone : MonoBehaviour
{
    private readonly HashSet<Actor> m_actorsInside = new();
    public event Action<Actor, bool> ActorEnterExit;
    protected void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void TryAddActor(Actor hitActor)
    {
        if(!m_actorsInside.Add(hitActor))
            return;

        hitActor.Destroyed += OnActorDestroyed;
        ActorEnterExit?.Invoke(hitActor, true);
    }
    private void TryRemoveActor(Actor hitActor)
    {
        if(!m_actorsInside.Remove(hitActor))
            return;
        
        hitActor.Destroyed -= OnActorDestroyed;
        ActorEnterExit?.Invoke(hitActor, false);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        var hitActor = col.GetComponentInParent<Actor>();
        if(!hitActor)
            return;
        
        TryAddActor(hitActor);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        var hitActor = col.GetComponentInParent<Actor>();
        if(!hitActor)
            return;
        
        TryRemoveActor(hitActor);
    }

    private void OnActorDestroyed(Actor actor) => TryRemoveActor(actor);

}
