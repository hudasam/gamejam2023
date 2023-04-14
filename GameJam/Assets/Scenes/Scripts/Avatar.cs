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
    private readonly MultiControl<PlayerAction> m_availableAction = new();
    private Rigidbody2D m_rigidbody;

    public Vector2 NavigationInput;
    public readonly Reactive<bool> JumpInput = new();

    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }
    
    public enum ActionPriority
    {
        Pickup = 0
    }
    
    public MultiControl<PlayerAction> AvailableAction => m_availableAction;
}
