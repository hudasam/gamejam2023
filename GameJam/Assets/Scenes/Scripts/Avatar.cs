using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.Utils;
using UnityEngine;

/// <summary>
/// Represents the player's playable avatar
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]

public class Avatar : Actor
{
    private readonly MultiControl<Action> m_availableAction = new();
    private Rigidbody2D m_rigidbody;

    
    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }
    
    public enum ActionPriority
    {
        Pickup = 0
    }
    
    public MultiControl<Action> AvailableAction => m_availableAction;
}
