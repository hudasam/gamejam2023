using System;
using SeweralIdeas.UnityUtils;
using SeweralIdeas.Utils;
using UnityEngine;
/// <summary>
/// Represents the player's playable avatar
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class Avatar : Actor
{
    [SerializeField] private float groundProximity = 0.1f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Collider2D[] m_colliders;
    [SerializeField] private Collider2D m_grounder;
    
    [Header("Movement")]
    [SerializeField] private float speed = 3;
    [SerializeField] private float maxAirSpeed = 6;
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float airAcceleration = 10;
    [SerializeField] private float jumpSpeed = 2f;
    [SerializeField] private float jumpAcceleration = 30f;
    [SerializeField] private float m_maxJumpFuel = 0.1f;
    [SerializeField] private float m_rollForce = 1f;

    [SerializeField] private PhysicsMaterial2D m_rollMaterial;
    [SerializeField] private PhysicsMaterial2D m_walkMaterial;
    [SerializeField] private Transform m_dontRoll;
    
    private readonly MultiControl<PlayerAction> m_availableAction = new();
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;

    public Vector2 NavigationInput;
    public readonly Reactive<bool> JumpInput = new();
    public readonly Reactive<bool> RollInput = new();

    private AnimatorFloat m_idGoingDirection = "GoingDirection";
    private AnimatorFloat m_idWalkSpeed = "WalkSpeed";
    private AnimatorBool m_idRolling = "Rolling";
    private AnimatorTrigger m_idJump = "Jump";

    private bool m_jumpInNext;
    private bool m_jumpQueued;
    private float m_jumpFuel;
    private readonly Collider2D[] m_colliderBuffer = new Collider2D[32];


    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        JumpInput.Changed += JumpInputChanged;
        RollInput.Changed += RollInputChanged;
        
        RollInputChanged(RollInput.Value);
    }
    
    private void RollInputChanged(bool roll)
    {
        var material = roll ? m_rollMaterial : m_walkMaterial;
        
        foreach (Collider2D coll in m_colliders)
            coll.sharedMaterial = material;

        m_rigidbody.constraints = roll ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeRotation;
    }

    public enum ActionPriority
    {
        Pickup = 0
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

    protected void Update()
    {
        if(Mathf.Abs(NavigationInput.x) > 0.01f)
        {
            var dir = Mathf.Sign(NavigationInput.x);
            m_animator.SetValue(m_idGoingDirection, dir);
        }

        float playWalk = (IsGrounded() && !RollInput.Value) ? Mathf.Abs(m_rigidbody.velocity.x) : 0f;
        m_animator.SetValue(m_idWalkSpeed, playWalk);
        m_animator.SetValue(m_idRolling, RollInput.Value);
        
        
        Debug.DrawLine(transform.position, transform.position - Vector3.up * groundProximity, IsGrounded()? Color.green : Color.red);
    }

    protected void LateUpdate()
    {
        m_dontRoll.rotation = Quaternion.identity;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(RollInput.Value)
        {
            Vector2 force = new()
            {
                x = NavigationInput.x * m_rollForce,
                y = 0
            };
            m_rigidbody.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            m_rigidbody.rotation = Mathf.MoveTowardsAngle(m_rigidbody.rotation, 0, Time.deltaTime * 360f);

            if(IsGrounded())
            {
                //walking
                float targetVelocityX = NavigationInput.x * speed;
                Vector2 velocity = m_rigidbody.velocity;
                velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
                m_rigidbody.velocity = velocity;
            }
            else
            {
                // air control
                if(Mathf.Abs(NavigationInput.x) > 0.1f)
                {
                    float targetVelocityX = NavigationInput.x * maxAirSpeed;
                    Vector2 velocity = m_rigidbody.velocity;
                    velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, airAcceleration * Time.fixedDeltaTime);
                    m_rigidbody.velocity = velocity;
                }
            }
        }
        
        HandleJumping();
    }

    void HandleJumping()
    {
        // handle input and queue
        if(IsGrounded(out var collider))
        {
            if(m_jumpInNext)
            {
                Jump(collider);

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
        
        // handle jump acceleration
        if(m_jumpFuel > 0f)
        {
            m_jumpFuel -= Time.fixedDeltaTime;
            var velocity = m_rigidbody.velocity;
            velocity.y += jumpAcceleration * Time.fixedDeltaTime;
            m_rigidbody.velocity = velocity;
        }
    }
    private void Jump(Collider2D standingOn)
    {
        m_animator.Trigger(m_idJump);
        //AddForce in Phys2D has no velocity options :(
        Vector2 velocity = new Vector2(m_rigidbody.velocity.x, 0);

        if(standingOn && standingOn.attachedRigidbody)
            velocity.y = standingOn.attachedRigidbody.velocity.y;

        velocity.y += jumpSpeed;
        m_rigidbody.velocity = velocity;
    }


    public MultiControl<PlayerAction> AvailableAction => m_availableAction;
    
    
    public bool IsGrounded() => IsGrounded(out _);
    
    public bool IsGrounded(out Collider2D collider)
    {
        var filter = new ContactFilter2D()
        {
            layerMask = groundMask,
            useLayerMask = true
        };
        var overlapCount = m_grounder.OverlapCollider(filter, m_colliderBuffer);

        collider = overlapCount > 0 ? m_colliderBuffer[0] : null;
        return overlapCount > 0;
    }
}
