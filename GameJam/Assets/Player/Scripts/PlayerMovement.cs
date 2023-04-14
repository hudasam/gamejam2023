using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(Avatar))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float airAcceleration = 10;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float jumpAcceleration = 10f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float m_maxJumpFuel = 0.5f;
    
    [SerializeField] private float groundProximity;
    [SerializeField] private LayerMask groundMask;
    private Avatar m_avatar;
    private bool m_jumpInNext;
    private bool m_jumpQueued;
    private float m_jumpFuel;
    

    private void Awake()
    {
        m_avatar = GetComponent<Avatar>();
        m_avatar.JumpInput.Changed += JumpInputChanged;
    }
    
    private void JumpInputChanged(bool pressed)
    {
        if(pressed)
        {
            if(IsGrounded())
            {
                m_jumpInNext = true;
            }
            else
            {
                m_jumpQueued = true;
            }
        }
        else
        {
            m_jumpQueued = false;
            m_jumpFuel = 0f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        HandleJumping();
    }

    void HandleMovement()
    {
        float targetVelocityX = m_avatar.NavigationInput.x * speed;
        float acc = IsGrounded() ? acceleration : airAcceleration;
        Vector2 velocity = rb.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acc * Time.fixedDeltaTime);
        rb.velocity = velocity;
    }

    void HandleJumping()
    {
        // handle input and queue
        if(IsGrounded(out var collider))
        {
            if(m_jumpInNext)
            {
                //AddForce in Phys2D has no velocity options :(
                Vector2 velocity = new Vector2(rb.velocity.x, 0);
                
                if(collider && collider.attachedRigidbody)
                    velocity.y = collider.attachedRigidbody.velocity.y;

                velocity.y += jumpSpeed;
                rb.velocity = velocity;
                
                m_jumpFuel = m_maxJumpFuel;
                m_jumpQueued = false;
                m_jumpInNext = false;
            }
            
            if(m_jumpQueued)
            {
                m_jumpQueued = false;
                m_jumpInNext = true;
            }
        }
        else
        {
            m_jumpInNext = false;
        }
        
        // handle acceleration
        if(m_jumpFuel > 0f)
        {
            m_jumpFuel -= Time.fixedDeltaTime;
            var velocity = rb.velocity;
            velocity.y += jumpAcceleration * Time.fixedDeltaTime;
            rb.velocity = velocity;
        }
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position - Vector3.up * groundProximity, IsGrounded()? Color.green : Color.red);
    }

    bool IsGrounded() => IsGrounded(out _);
    
    bool IsGrounded(out Collider2D collider)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, groundProximity, groundMask);
        collider = hit.collider;
        return hit;
    }
}