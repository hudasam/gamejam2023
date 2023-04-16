using SeweralIdeas.UnityUtils;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Beetle : Actor, INeedleDamageReceiver
{
    [SerializeField] private int m_hitPoints = 3;
    [SerializeField] private bool m_goingLeft;
    [SerializeField] private LayerMask m_turnAroundCollisionMask;
    [SerializeField] private Transform m_flip;
    [SerializeField] private Collider2D m_frontCollisionSensor;
    [SerializeField] private Zone m_attackTrigger;

    [SerializeField] private Vector2 m_punchDeltaV;
    [SerializeField] private float m_punchKnockoutDuration = 2f;

    private bool m_dead;
    private Animator m_animator;
    
    [Header("Movement")]
    [SerializeField] private float m_speed = 3f;
    [SerializeField] private float m_acceleration = 3f;

    private AudioSource soundSrc;

    private static readonly AnimatorFloat m_walkSpeed = "WalkSpeed";
    private static readonly AnimatorTrigger m_attack = "Attack";
    private static readonly AnimatorTrigger m_pain = "Pain";
    
    private bool m_alive = true;
    private Rigidbody2D m_rigidbody;

    [SerializeField] private GameObject[] m_deactivateOnDeath;

    protected void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_attackTrigger.ActorEnterExit += OnAttackTriggerEnterExit;
        soundSrc = GetComponent<AudioSource>();
    }
    
    private void OnAttackTriggerEnterExit(Actor actor, bool entered)
    {
        if(m_dead)
            return;

        if(entered && actor is Avatar avatar)
        {
            PlayPunchEffect();
            var punchDeltaV = m_punchDeltaV;

            if(actor.transform.position.x < transform.position.x)
                punchDeltaV.x *= -1;
            avatar.ReceivePunch(this, punchDeltaV, m_punchKnockoutDuration, false);
        }
    }


    protected void FixedUpdate()
    {
        if(m_dead)
            return;
        
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
        
        if(ColliderOverlapsSomething(m_frontCollisionSensor, m_turnAroundCollisionMask, out var hit))
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
    
    public void ReceiveNeedleDamage(Actor inflictor)
    {
        if(m_dead)
            return;

        soundSrc.Play();
        
        m_hitPoints--;
        if(m_hitPoints <= 0)
        {
            m_dead = true;
            float horizontal = Mathf.Sign(transform.position.x - inflictor.transform.position.x) * 1f;
            m_rigidbody.velocity = new Vector2(horizontal, 5);
            m_flip.transform.localRotation = Quaternion.Euler(180f, m_goingLeft? 180:0, 0f);
            foreach (var obj in m_deactivateOnDeath)
            {
                obj.SetActive(false);
            }
            gameObject.AddComponent<MaxLifetime>().TimeLeft = 3f;
        }
        else
        {
            // pain
            m_animator.Trigger(m_pain);
        }
    }
}
