using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    [SerializeField] private Vector2 m_velocity;
    [SerializeField] private float m_knockoutDuration;
    [SerializeField] private bool m_addVelocity = false;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.isTrigger)
            return;
        
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
