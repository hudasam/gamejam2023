using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Represents dynamic world object
/// </summary>
[DisallowMultipleComponent]
public class Actor : MonoBehaviour
{
    [SerializeField] private Collider2D m_grounder;
    
    public event Action<Actor> Destroyed;
    
    protected virtual void OnDestroy()
    {
        Destroyed?.Invoke(this);   
    }
    
    
    protected bool IsGrounded() => IsGrounded(out _);

    protected bool IsGrounded(out Collider2D collider) => ColliderOverlapsSomething(m_grounder, GlobalSettings.GetInstance().GroundMask, out collider);
    
    protected bool ColliderOverlapsSomething(Collider2D sensor, LayerMask layerMask, out Collider2D collider)
    {
        var filter = new ContactFilter2D()
        {
            layerMask = layerMask,
            useLayerMask = true
        };

        using (ListPool<Collider2D>.Get(out var colliderBuffer))
        {
            sensor.OverlapCollider(filter, colliderBuffer);

            collider = colliderBuffer.Count > 0 ? colliderBuffer[0] : null;
            return colliderBuffer.Count > 0;
        }
    }

    protected void ColliderOverlapActors<T>(Collider2D sensor, ContactFilter2D filter, HashSet<T> result) where T:Actor
    {
        using(ListPool<Collider2D>.Get(out var attackColliders))
        {
            sensor.OverlapCollider(filter, attackColliders);
            foreach (Collider2D collider in attackColliders)
            {
                var actor = collider.GetComponentInParent<Actor>(); // not T, detect other actors too
                if(actor is T typed)
                {
                    result.Add(typed);
                }
            }
        }
    }

}
