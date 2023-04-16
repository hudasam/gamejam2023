using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeweralIdeas.Pooling;
using SeweralIdeas.UnityUtils;
using SeweralIdeas.Utils;


[RequireComponent(typeof(Animator))]
public class Rat : Actor, INeedleDamageReceiver
{
    


    [SerializeField] private Collider2D m_frontCollisionSensor;
    [SerializeField] private Zone m_attackTrigger;
    [SerializeField] private Zone m_actionTrigger;

    [SerializeField] private Vector2 m_punchDeltaV;
    [SerializeField] private float m_punchKnockoutDuration = 2f;
    [SerializeField] private float ratKnockOutTime=1.5f;
    bool active;

    private int attackAnim = Animator.StringToHash("Attack");
    private int knockOutAnim = Animator.StringToHash("knockedOut");
    private int sewAnim = Animator.StringToHash("Sewed");

    private AudioSource soundSrc;
    private SpriteRenderer sprite;
    private Coroutine knockOutTimerCoroutine;

    [SerializeField] private int m_contextActionPrioriy;
    [SerializeField] private Animator m_animator;

    [SerializeField] private PlayerAction m_playerAction;
    private Dictionary<Avatar, MultiControl<(PlayerAction, Transform)>.Request> m_actionRequests = new();


    private bool knockedOut;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        m_attackTrigger.ActorEnterExit += OnAttackTriggerEnterExit;
        m_actionTrigger.ActorEnterExit += OnActionTriggerEnterExit;
        m_animator = GetComponent<Animator>();
        active = true;
        soundSrc = GetComponent<AudioSource>();
        sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        sprite.enabled = false;
    }

    private void OnAttackTriggerEnterExit(Actor actor, bool entered)
    {
        if (knockedOut) return;
        if (entered && actor is Avatar avatar)
        {
            PlayPunchEffect();
            Vector2 dir = this.gameObject.transform.position - actor.gameObject.transform.position;
            dir = dir.normalized;
            dir.y = 0;
            dir *= m_punchDeltaV;
            avatar.ReceivePunch(this, dir, m_punchKnockoutDuration, false);
        }
    }
    private void PlayPunchEffect() 
    {
        if (m_animator.enabled == false)
        {
            sprite.enabled = true;
            m_animator.enabled = true;
        }
        else
            m_animator.SetTrigger(attackAnim);     
    }
    private void OnActionTriggerEnterExit(Actor actor, bool entered) 
    {
        
        if (actor is Avatar avatar)
        {
            //Item check TODO
            
            if (entered)
            {
                
                if (!active) return;
                if (m_playerAction == null)
                    Debug.LogError($"{nameof(m_playerAction)} is null", gameObject);
                var request = avatar.AvailableAction.CreateRequest(name, m_contextActionPrioriy, (m_playerAction, transform), true);
                m_actionRequests.Add(avatar, request);
            }
            else
            {
                var request = m_actionRequests[avatar];
                request.Dispose();
                m_actionRequests.Remove(avatar);
            }

        }
    }

    public bool SewUp() 
    {
        if (!knockedOut) return false;
        if (knockedOut)
        {

            active = false;
            m_animator.SetTrigger(sewAnim);

            StartCoroutine(Timer());
            

        }
        return true;
    }
    IEnumerator Timer() 
    {
        float t = 2f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        DisableRat();
    }
    private void DisableRat() 
    {
        m_attackTrigger.gameObject.SetActive(false);
        //m_actionTrigger.gameObject.SetActive(false);
        transform.Find("Sprite").gameObject.SetActive(false);
        transform.Find("Collider").gameObject.SetActive(false);
        transform.Find("Cover").gameObject.SetActive(true);

    }


    private IEnumerator KnockOutTimer()
    {
        float t = ratKnockOutTime;
        knockedOut = true;
        soundSrc.Play();
        m_animator.SetBool(knockOutAnim,true);
        while (t > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        knockedOut = false;
        m_animator.SetBool(knockOutAnim, false);
        knockOutTimerCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void INeedleDamageReceiver.ReceiveNeedleDamage(Actor inflictor)
    {
        if (knockOutTimerCoroutine == null)
            knockOutTimerCoroutine = StartCoroutine(KnockOutTimer());
        
        //Debug.Log("rat hit");
    }
}
