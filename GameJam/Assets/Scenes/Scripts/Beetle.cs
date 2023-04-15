using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeweralIdeas.Pooling;
using SeweralIdeas.UnityUtils;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Beetle : Actor
{
    [SerializeField] private bool m_goingLeft;
    [SerializeField] private LayerMask m_turnAroundCollisionMask;
    [SerializeField] private Transform m_flip;
    [SerializeField] private Collider2D m_frontCollisionSensor;
    [SerializeField] private Zone m_attackTrigger;

    [SerializeField] private Vector2 m_punchDeltaV;
    [SerializeField] private float m_punchKnockoutDuration = 2f;

    private Animator m_animator;
    
    [Header("Movement")]
    [SerializeField] private float m_speed = 3f;
    [SerializeField] private float m_acceleration = 3f;


    private static readonly AnimatorFloat m_walkSpeed = "WalkSpeed";
    private static readonly AnimatorTrigger m_attack = "Attack";
    
    private bool m_alive = true;
    private Rigidbody2D m_rigidbody;


    protected void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_attackTrigger.ActorEnterExit += OnAttackTriggerEnterExit;
    }
    
    private void OnAttackTriggerEnterExit(Actor actor, bool entered)
    {
        if(entered && actor is Avatar avatar)
        {
            PlayPunchEffect();
            avatar.ReceivePunch(this, m_punchDeltaV, m_punchKnockoutDuration);
        }
    }


    protected void FixedUpdate()
    {
        if(IsGrounded())
        {
            float targetSpeed = m_goingLeft ? -m_speed : m_speed;
        
            //walking
            Vector2 velocity = m_rigidbody.velocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, m_acceleration * Time.fixedDeltaTime);
            m_rigidbody.velocity = velocity;
            m_animator.SetValue(m_walkSpeed, Mathf.Abs(velocity.x));
        }
        else
        {
            m_animator.SetValue(m_walkSpeed, 0f);
        }
        
        if(ColliderOverlapsSomething(m_frontCollisionSensor, out var hit))
        {
            m_goingLeft = !m_goingLeft;
        }
        
        m_flip.transform.localRotation = Quaternion.Euler(0, m_goingLeft? 180:0, 0f);
    }
    
    private void PlayPunchEffect()
    {
        m_animator.Trigger(m_attack);
        Debug.Log("TODO pum=nch effect animation", gameObject);
    }
}
