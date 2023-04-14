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
    
    [Header("Movement")]
    [SerializeField] private float speed = 3;
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float airAcceleration = 10;
    [SerializeField] private float jumpSpeed = 2f;
    [SerializeField] private float jumpAcceleration = 30f;
    [SerializeField] private float m_maxJumpFuel = 0.1f;

    
    private readonly MultiControl<PlayerAction> m_availableAction = new();
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;

    public Vector2 NavigationInput;
    public readonly Reactive<bool> JumpInput = new();

    private AnimatorFloat m_idGoingDirection = "GoingDirection";
    private AnimatorFloat m_idWalkSpeed = "WalkSpeed";
    private AnimatorTrigger m_idJump = "Jump";

    private bool m_jumpInNext;
    private bool m_jumpQueued;
    private float m_jumpFuel;


    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        JumpInput.Changed += JumpInputChanged;
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
        
        m_animator.SetValue(m_idWalkSpeed, IsGrounded()? Mathf.Abs(m_rigidbody.velocity.x) : 0f);
        
        
        Debug.DrawLine(transform.position, transform.position - Vector3.up * groundProximity, IsGrounded()? Color.green : Color.red);
    }
    
    
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        HandleJumping();
    }

    void HandleMovement()
    {
        float targetVelocityX = NavigationInput.x * speed;
        float acc = IsGrounded() ? acceleration : airAcceleration;
        Vector2 velocity = m_rigidbody.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acc * Time.fixedDeltaTime);
        m_rigidbody.velocity = velocity;
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
        
        // handle acceleration
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, groundProximity, groundMask);
        collider = hit.collider;
        return hit;
    }
}
