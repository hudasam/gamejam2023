using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using SeweralIdeas.UnityUtils.Drawers;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Moth : Actor
{
    [SerializeField] private float m_hivRadius = 2f;

    [SerializeField] private float m_moveSpeed;
    [SerializeField, MinMaxSlider(0f, 5f)] private Vector2 m_stopDurationInterval;
    [SerializeField, MinMaxSlider(0f, 5f)] private Vector2 m_moveDurationInterval;
    [SerializeField] private float m_flightSpeed = 2f;
    [SerializeField] private Vector2 m_flightAngVelocityRange = new Vector2(-60f, 60f);
    
    private Vector2 m_hive;
    private Vector2 m_destination;
    private Rigidbody2D m_rigidbody;
    private float m_stopDuration;
    private float m_moveDuration;
    private Avatar m_collector;
    private float m_flightAngularVelocity;
    private float m_flightChangeInterval;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_destination = m_rigidbody.position;
    }

    private void Start()
    {
        m_hive = m_rigidbody.position;
        m_rigidbody.position = GetRandomPoint();
    }

    private void OnValidate()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_hivRadius);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if(m_collector)
            return;
        
        var actor = other.GetComponentInParent<Actor>();
        if(!actor)
            return;
        if(!(actor is Avatar avatar))
            return;
        
        m_collector = avatar;
        m_moveDuration = 0;
        gameObject.AddComponent<MaxLifetime>().TimeLeft = 5f;
    }

    private void FixedUpdate()
    {
        if(m_collector)
        {
            if(m_moveDuration <= 0)
            {
                m_flightAngularVelocity = Random.Range(m_flightAngVelocityRange.x, m_flightAngVelocityRange.y);
                if(m_flightChangeInterval >= 0.1)
                    m_flightAngularVelocity *= 0.05f;
                m_moveDuration = m_flightChangeInterval + Random.Range(0.01f, 0.2f);
                m_flightChangeInterval += 0.02f;
            }

            m_moveDuration -= Time.fixedDeltaTime;
            m_rigidbody.rotation += m_flightAngularVelocity * Time.fixedDeltaTime;
            float angRad = m_rigidbody.rotation * Mathf.Deg2Rad;
            m_rigidbody.position += new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad)) * (Time.fixedDeltaTime * m_flightSpeed);
            return;
        }

        if(m_moveDuration > 0)
        {
            if((m_destination - m_rigidbody.position).sqrMagnitude < 0.1f)
            {
                m_destination = GetRandomPoint();
                var offset = m_destination - m_rigidbody.position;
                m_rigidbody.rotation = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            }
            
            m_moveDuration -= Time.fixedDeltaTime;
            if(m_moveDuration <= 0)
            {
                m_stopDuration = Random.Range(m_stopDurationInterval.x, m_stopDurationInterval.y);
            }

            m_rigidbody.position = Vector2.MoveTowards(m_rigidbody.position, m_destination, Time.fixedDeltaTime * m_moveSpeed);
        }
        else
        {
            m_stopDuration -= Time.fixedDeltaTime;
            if(m_stopDuration <= 0)
            {
                m_moveDuration = Random.Range(m_moveDurationInterval.x, m_moveDurationInterval.y);
            }
        }
    }
    private Vector2 GetRandomPoint()
    {

        return Random.insideUnitCircle * m_hivRadius + m_hive;
    }
}
