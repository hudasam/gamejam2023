using System;
using System.Collections.Generic;
using SeweralIdeas.StateMachines;
using SeweralIdeas.UnityUtils;
using SeweralIdeas.Utils;
using UnityEngine;
using UnityEngine.Serialization;
/// <summary>
/// Represents the player's playable avatar
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class Avatar : Actor
{
    [SerializeField] private float m_groundProximity = 0.3f;
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private Collider2D[] m_colliders;
    [SerializeField] private Collider2D m_grounder;
    [SerializeField] private RopeScript m_ropePrefab;
    
    [Header("Movement")]
    [SerializeField] private float m_walkSpeed = 3;
    [SerializeField] private float m_maxAirSpeed = 6;
    [SerializeField] private float m_walkAcceleration = 50;
    [SerializeField] private float m_airAcceleration = 10;
    [SerializeField] private float m_ropeAirAcceleration = 10;
    [SerializeField] private float m_maxRopeAirSpeed = 6;
    [SerializeField] private float m_jumpSpeed = 2f;
    [SerializeField] private float m_jumpBoostAcceleration = 30f;
    [SerializeField] private float m_jumpBoostDuration = 0.1f;
    [SerializeField] private float m_rollForce = 1f;

    [SerializeField] private float m_maxRopeDistance;
    
    [SerializeField] private PhysicsMaterial2D m_rollMaterial;
    [SerializeField] private PhysicsMaterial2D m_walkMaterial;
    [SerializeField] private Transform m_dontRoll;

    private StateMachine m_machine = new("Avatar", new State_Root());
    
    private readonly MultiControl<PlayerAction> m_availableAction = new();
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;

    [NonSerialized] public float NavigationInput;
    public readonly Reactive<bool> JumpInput = new();
    public readonly Reactive<bool> RollInput = new();

    private static readonly AnimatorFloat s_idGoingDirection = "GoingDirection";
    private static readonly AnimatorFloat s_idWalkSpeed = "WalkSpeed";
    private static readonly AnimatorBool s_idRolling = "Rolling";
    private static readonly AnimatorTrigger s_idJump = "Jump";

    private bool m_jumpInNext;
    private bool m_jumpQueued;
    private float m_jumpFuel;
    private readonly Collider2D[] m_colliderBuffer = new Collider2D[32];

    private interface ITick : IStateBase { void Tick(float deltaTime); }
    private interface IUpdate : IStateBase { void Update(float deltaTime); }
    private interface IThrowRope { void ThrowRope(Vector2 worldPosition); }

    private static readonly Handler<IUpdate, float> msg_update = (handler, deltaTime) => { handler.state.PropagateMessage(); handler.Update(deltaTime); };
    private static readonly Handler<ITick, float> msg_tick = (handler, deltaTime) => { handler.state.PropagateMessage(); handler.Tick(deltaTime); };
    private static readonly Handler<IThrowRope, Vector2> msg_throwRope = (handler, worldPosition) => { handler.ThrowRope(worldPosition); };
    
    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        JumpInput.Changed += JumpInputChanged;
        m_machine.Initialize(this);
    }

    protected override void OnDestroy()
    {
        m_machine.Shutdown();
        base.OnDestroy();
    }


    private void SetPhysMaterial(PhysicsMaterial2D material)
    {
        foreach (Collider2D coll in m_colliders)
            coll.sharedMaterial = material;
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
        m_machine.SendMessage(msg_update, Time.deltaTime);

        if(Input.GetMouseButtonDown(0) && GetDestination(out var worldPos))
        {
            m_machine.SendMessage(msg_throwRope, worldPos);
        }
    }
    
    bool GetDestination(out Vector2 anchorDest) 
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        Vector2 targetDir = (mousePos - playerPos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(playerPos, targetDir, m_maxRopeDistance, m_groundMask);
        Debug.DrawRay(transform.position, targetDir, Color.green);
        
        if (hit.collider != null ) 
        {
            anchorDest = hit.point;
            return true;
        }
        
        anchorDest = Vector2.zero;
        return false;
    }

    protected void LateUpdate()
    {
        m_dontRoll.rotation = Quaternion.identity;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        m_machine.SendMessage(msg_tick, Time.fixedDeltaTime);
    }

    private void HandleWalking(float maxSpeed, float walkAcceleration, float maxAirSpeed, float airAcceleration)
    {
        if(IsGrounded())
        {
            //walking
            float targetVelocityX = NavigationInput * maxSpeed;
            Vector2 velocity = m_rigidbody.velocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, walkAcceleration * Time.fixedDeltaTime);
            m_rigidbody.velocity = velocity;
        }
        else
        {
            // air control
            if(Mathf.Abs(NavigationInput) > 0.1f)
            {
                float targetVelocityX = NavigationInput * maxAirSpeed;
                Vector2 velocity = m_rigidbody.velocity;
                velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, airAcceleration * Time.fixedDeltaTime);
                m_rigidbody.velocity = velocity;
            }
        }
    }

    void HandleJumping()
    {
        // handle input and queue
        if(IsGrounded(out var collider))
        {
            if(m_jumpInNext)
            {
                Jump(collider);

                m_jumpFuel = m_jumpBoostDuration;
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
            velocity.y += m_jumpBoostAcceleration * Time.fixedDeltaTime;
            m_rigidbody.velocity = velocity;
        }
    }
    
    private void Jump(Collider2D standingOn)
    {
        m_animator.Trigger(s_idJump);
        //AddForce in Phys2D has no velocity options :(
        Vector2 velocity = new Vector2(m_rigidbody.velocity.x, 0);

        if(standingOn && standingOn.attachedRigidbody)
            velocity.y = standingOn.attachedRigidbody.velocity.y;

        velocity.y += m_jumpSpeed;
        m_rigidbody.velocity = velocity;
    }


    public MultiControl<PlayerAction> AvailableAction => m_availableAction;
    
    
    public bool IsGrounded() => IsGrounded(out _);
    
    public bool IsGrounded(out Collider2D collider)
    {
        var filter = new ContactFilter2D()
        {
            layerMask = m_groundMask,
            useLayerMask = true
        };
        var overlapCount = m_grounder.OverlapCollider(filter, m_colliderBuffer);

        collider = overlapCount > 0 ? m_colliderBuffer[0] : null;
        return overlapCount > 0;
    }

    class State_Root : HierarchicalState<Avatar>, IState, IUpdate, IThrowRope
    {
        private readonly State_Walking m_state_walking = new();
        private readonly State_Roped m_state_roped = new();
        private readonly State_Rolling m_state_rolling = new();

        void IThrowRope.ThrowRope(Vector2 worldPos) => TransitTo(m_state_roped, worldPos);

        protected override void OnInitialize(out IState entrySubState, List<IStateBase> subStates)
        {
            subStates.Add(m_state_walking);
            subStates.Add(m_state_roped);
            subStates.Add(m_state_rolling);
            entrySubState = m_state_walking;
        }
        
        void IState.Enter() { }

        void IUpdate.Update(float deltaTime)
        {
            if(Mathf.Abs(actor.NavigationInput) > 0.01f)
            {
                var dir = Mathf.Sign(actor.NavigationInput);
                actor.m_animator.SetValue(s_idGoingDirection, dir);
            }
        }
        
        class State_Walking : SimpleState<Avatar, State_Root>, IState, ITick, IUpdate
        {
            void IState.Enter() { }
            
            protected override void OnEnter()
            {
                base.OnEnter();
                actor.m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                actor.SetPhysMaterial(actor.m_walkMaterial);
            }

            void ITick.Tick(float deltaTime)
            {
                actor.m_rigidbody.rotation = Mathf.MoveTowardsAngle(actor.m_rigidbody.rotation, 0, Time.deltaTime * 360f);

                actor.HandleWalking(actor.m_walkSpeed, actor.m_walkAcceleration, actor.m_maxAirSpeed, actor.m_airAcceleration);
                actor.HandleJumping();
                
                if(actor.RollInput.Value)
                {
                    TransitTo(parent.m_state_rolling);
                }
            }
            
            void IUpdate.Update(float deltaTime)
            {
                float playWalk = actor.IsGrounded() ? Mathf.Abs(actor.m_rigidbody.velocity.x) : 0f;
                actor.m_animator.SetValue(s_idWalkSpeed, playWalk);
            }

            protected override void OnExit()
            {
                base.OnExit();
                actor.m_animator.SetValue(s_idWalkSpeed, 0f);
            }
        }
        
        class State_Roped : SimpleState<Avatar, State_Root>, IState<Vector2>, ITick, IUpdate
        {
            private RopeScript m_rope;
            
            void IState<Vector2>.Enter(Vector2 worldPosition)
            {
                m_rope = Instantiate(actor.m_ropePrefab, actor.transform.position, Quaternion.identity);
                m_rope.anchorDest = worldPosition;
            }
            
            protected override void OnEnter()
            {
                base.OnEnter();
                actor.m_rigidbody.constraints = RigidbodyConstraints2D.None;
                actor.SetPhysMaterial(actor.m_rollMaterial);
            }

            protected override void OnExit()
            {
                if(m_rope)
                    Destroy(m_rope.gameObject);
                base.OnExit();
            }

            void ITick.Tick(float deltaTime)
            {
                actor.m_rigidbody.rotation = Mathf.MoveTowardsAngle(actor.m_rigidbody.rotation, 0, Time.deltaTime * 360f);

                actor.HandleWalking(actor.m_walkSpeed, actor.m_walkAcceleration, actor.m_maxRopeAirSpeed, actor.m_ropeAirAcceleration);
                actor.HandleJumping();
                
                // if(actor.RollInput.Value)
                // {
                //     TransitTo(parent.m_state_rolling);
                // }
            }

            void IUpdate.Update(float deltaTime)
            {
                if(Input.GetMouseButtonDown(1))
                {
                    TransitTo(parent.m_state_walking);
                }
            }
        }
        
        class State_Rolling : SimpleState<Avatar, State_Root>, IState, ITick
        {
            void IState.Enter() { }

            protected override void OnEnter()
            {
                base.OnEnter();
                actor.m_rigidbody.constraints = RigidbodyConstraints2D.None;
                actor.SetPhysMaterial(actor.m_rollMaterial);
                actor.m_animator.SetValue(s_idRolling, true);
            }

            protected override void OnExit()
            {
                actor.m_animator.SetValue(s_idRolling, false);
                base.OnExit();
            }

            void ITick.Tick(float deltaTime)
            {
                Vector2 force = new()
                {
                    x = actor.NavigationInput * actor.m_rollForce,
                    y = 0
                };
                actor.m_rigidbody.AddForce(force, ForceMode2D.Force);

                actor.HandleJumping();
                
                
                if(!actor.RollInput.Value)
                {
                    TransitTo(parent.m_state_walking);
                }
            }
        }
    }
}
