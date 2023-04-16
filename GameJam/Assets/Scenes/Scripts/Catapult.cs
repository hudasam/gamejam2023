using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    [SerializeField] private Vector2 m_velocity;
    [SerializeField] private float m_knockoutDuration;
    [SerializeField] private bool m_addVelocity = false;
    [SerializeField] private Animator m_animator;
    [SerializeField] private AudioSource m_audio;
    private static readonly AnimatorTrigger s_fire = "Fire";
    
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.isTrigger)
            return;
        
        if(m_animator)
            m_animator.Trigger(s_fire);
        if(m_audio)
            m_audio.Play();
        var avatar = col.GetComponentInParent<Avatar>();

        Vector2 velocity = transform.TransformDirection(m_velocity);
        
        if(avatar)
        {
            avatar.ReceivePunch(null, velocity, m_knockoutDuration, m_addVelocity);
            return;
        }
        
        // rigidbody fallback
        var rb = col.attachedRigidbody;
        if(!rb)
            return;

        if(m_addVelocity)
            rb.velocity += velocity;
        else
            rb.velocity = velocity;
    }
}
