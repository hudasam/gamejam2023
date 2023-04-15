using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeweralIdeas.Pooling;
using SeweralIdeas.UnityUtils;
using SeweralIdeas.Utils;

public class Rat : Actor
{
    [SerializeField] private Collider2D m_frontCollisionSensor;
    [SerializeField] private Zone m_attackTrigger;
    [SerializeField] private Zone m_actionTrigger;

    [SerializeField] private Vector2 m_punchDeltaV;
    [SerializeField] private float m_punchKnockoutDuration = 2f;

    [SerializeField] private PlayerAction m_playerAction;
    private Dictionary<Avatar, MultiControl<(PlayerAction, Transform)>.Request> m_actionRequests = new();
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        m_attackTrigger.ActorEnterExit += OnAttackTriggerEnterExit;
        m_actionTrigger.ActorEnterExit += OnActionTriggerEnterExit;
    }

    private void OnAttackTriggerEnterExit(Actor actor, bool entered)
    {
        if (entered && actor is Avatar avatar)
        {
            //PlayPunchEffect();
            Vector2 dir = this.gameObject.transform.position - actor.gameObject.transform.position;
            dir = dir.normalized;
            dir.y = 0;
            dir *= m_punchDeltaV;
            avatar.ReceivePunch(this, dir, m_punchKnockoutDuration);
        }
    }

    private void OnActionTriggerEnterExit(Actor actor, bool entered) 
    {
        if ((entered && actor is Avatar avatar))
        {
            //Item check TODO
            if (entered)
            {
                if (m_playerAction == null)
                    Debug.LogError($"{nameof(m_playerAction)} is null", gameObject);
                var request = avatar.AvailableAction.CreateRequest(name, (int)Avatar.ActionPriority.Swing, (m_playerAction, transform), true);
                m_actionRequests[avatar] = request;
            }
            else
            {
                var request = m_actionRequests[avatar];
                request.Dispose();
                m_actionRequests.Remove(avatar);
            }

        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
