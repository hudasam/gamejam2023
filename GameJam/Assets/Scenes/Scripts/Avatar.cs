using System;
using System.Collections.Generic;
using SeweralIdeas.Pooling;
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
    [SerializeField] private Collider2D[] m_colliders;
    
    [SerializeField] private RopeScript m_ropePrefab;
    
    [Header("Movement")]
    [SerializeField] private float m_walkSpeed = 3;
    [SerializeField] private float m_maxAirSpeed = 6;
    [SerializeField] private float m_walkAcceleration = 50;
    [SerializeField] private float m_walkDecceleration = 10;
    [SerializeField] private float m_airAcceleration = 10;
    [SerializeField] private float m_ropeAirAcceleration = 10;
    [SerializeField] private float m_maxRopeAirSpeed = 6;
    [SerializeField] private float m_jumpSpeed = 2f;
    [SerializeField] private float m_jumpBoostAcceleration = 30f;
    [SerializeField] private float m_jumpBoostDuration = 0.1f;
    [SerializeField] private float m_wallJumpSpeed = 3;
    
    [SerializeField] private float m_maxRopeDistance;
    [SerializeField] private Transform m_flip;
    
    [SerializeField] private PhysicsMaterial2D m_rollMaterial;
    [SerializeField] private PhysicsMaterial2D m_walkMaterial;
    [SerializeField] private Transform m_dontRoll;
    [SerializeField] private HintUI m_hintsUI;
    [SerializeField] private Collider2D m_attackArea;
    
    private StateMachine m_machine = new("Avatar", new State_Root());


    [SerializeField] private PlayerHint m_needleHint;
    [SerializeField] private PlayerHint m_threadHint;
    
    private readonly MultiControl<(PlayerAction act,Transform transform)> m_availableAction = new();

    private Rigidbody2D m_rigidbody;
    private Animator m_animator;

    [NonSerialized] public float NavigationInput;
    public readonly Reactive<bool> JumpInput = new();
    public readonly Reactive<bool> AttackInput = new();
    public readonly Reactive<bool> ContextInput = new();

    private bool m_hasNeedle = false;
    private bool m_hasThread = false;

    private static readonly AnimatorFloat s_idGoingDirection = "GoingDirection";
    private static readonly AnimatorFloat s_idWalkSpeed = "WalkSpeed";
    private static readonly AnimatorBool s_idRolling = "Rolling";
    private static readonly AnimatorBool s_idRoped = "Roped";
    private static readonly AnimatorBool s_idHasNeedle = "HasNeedle";
    private static readonly AnimatorBool s_idHasThread = "HasThread";
    private static readonly AnimatorTrigger s_idJump = "Jump";
    private static readonly AnimatorTrigger s_idAttack = "Attack";

    private bool m_jumpInNext;
    private bool m_jumpQueued;
    private float m_jumpFuel;
    private float m_orientation;
    
    private interface ITick : IStateBase { void Tick(float deltaTime); }
    private interface IUpdate : IStateBase { void Update(float deltaTime); }
    private interface IThrowRope { void ThrowRope(Vector2 worldPosition); }
    private interface IReceivePunch { void ReceivePunch(Actor inflictor, Vector2 velocity, float knockoutDuration); }
    private interface IAttack { void Attack(); }

    private static readonly Handler<IUpdate, float> msg_update = (handler, deltaTime) => { handler.state.PropagateMessage(); handler.Update(deltaTime); };
    private static readonly Handler<ITick, float> msg_tick = (handler, deltaTime) => { handler.state.PropagateMessage(); handler.Tick(deltaTime); };
    private static readonly Handler<IThrowRope, Vector2> msg_throwRope = (handler, worldPosition) => { handler.ThrowRope(worldPosition); };
    private static readonly Handler<IReceivePunch, (Actor inflictor, Vector2 velocity, float knockoutDuration)> msg_receivePunch = (handler, args) => { handler.ReceivePunch(args.inflictor, args.velocity, args.knockoutDuration); };
    private static readonly Handler<IAttack> msg_attack = (handler) => handler.Attack();
    
    [SerializeField]
    private double m_attackCooldown = 0.25f;
    

    protected void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        JumpInput.Changed += JumpInputChanged;
        
        AttackInput.Changed += (bool isOn) =>
        {
            if(isOn) m_machine.SendMessage(msg_attack);
        };
        
        HasNeedle = false;
        HasThread = false;
        
        m_machine.Initialize(this);
        ContextInput.Changed += ContextInputChanged;

    }
    
    public bool HasThread
    {
        get => m_hasThread;
        set
        {
            m_hasThread = value;
            m_animator.SetValue(s_idHasThread, value);
            if(m_hasThread)
                m_hintsUI.MaskHint(m_threadHint);
            else 
                m_hintsUI.UnmaskHint(m_threadHint);
            
        }
    }
    
    public bool HasNeedle
    {
        get => m_hasNeedle;
        set
        {
            m_hasNeedle = value;
            m_animator.SetValue(s_idHasNeedle, value);
            if(m_hasNeedle)
                m_hintsUI.MaskHint(m_needleHint);
            else 
                m_hintsUI.UnmaskHint(m_needleHint);
        }
    }

    private void UpdateHints()
    {
        if (HasNeedle) m_hintsUI.MaskHint(m_needleHint);
        else m_hintsUI.UnmaskHint(m_needleHint);

        if (HasThread) m_hintsUI.MaskHint(m_threadHint);
        else m_hintsUI.UnmaskHint(m_threadHint);

        if (HasThread && HasNeedle) ;//TODO make thread and needle hint
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
        Pickup = 0,
        Sewing = 1,
        Swing = 2
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

    private void ContextInputChanged(bool pressed) 
    {
        if(pressed)
        {
            if (AvailableAction != null) 
            {
                HandleAction();
            }
        }
    }

    private void HandleAction()
    {
        if(AvailableAction.Value.act == null)
            return;
        
        switch (AvailableAction.Value.act.Type)
        {
            case PlayerAction.ActionType.Swing:
                //Debug.Log("Send rope");
                if(HasThread)
                m_machine.SendMessage(msg_throwRope, AvailableAction.Value.transform.position);
                break;
            case PlayerAction.ActionType.Use:
                if (HasThread && HasNeedle)
                    if (AvailableAction.Value.transform.GetComponentInParent<Actor>() is Rat) 
                    {
                        Rat r =(Rat)AvailableAction.Value.transform.GetComponentInParent<Actor>();
                        if(r.SewUp())
                            ConsumeItems();
                    }
                    
                break;
        }
    }
    private void ConsumeItems()
    {
        //HasNeedle = false;
        HasThread = false;
    }

    protected void Update()
    {
        m_machine.SendMessage(msg_update, Time.deltaTime);
    }
    
    bool GetDestination(out Vector2 anchorDest) 
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        Vector2 targetDir = (mousePos - playerPos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(playerPos, targetDir, m_maxRopeDistance, GlobalSettings.GetInstance().GroundMask);
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

    private void HandleWalking(float maxSpeed, float walkAcceleration, float walkDecceleration, float maxAirSpeed, float airAcceleration)
    {
        if(IsGrounded())
        {
            //walking
            float targetVelocityX = NavigationInput * maxSpeed;
            Vector2 velocity = m_rigidbody.velocity;
            float acceleration = NavigationInput == 0 || velocity.x * NavigationInput > 0.1f ? walkAcceleration : walkDecceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
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



    public MultiControl<(PlayerAction act,Transform transform)> AvailableAction => m_availableAction;

    public HintUI Hints => m_hintsUI;

    class State_Root : HierarchicalState<Avatar>, IState, IUpdate, IThrowRope, IReceivePunch, IAttack
    {
        private readonly State_Walking m_state_walking = new();
        private readonly State_Roped m_state_roped = new();
        private readonly State_KnockedOut m_state_knockedOut = new();

        private float m_lastAttackTime;
        private float m_flipRotation;
        
        void IThrowRope.ThrowRope(Vector2 worldPos) => TransitTo(m_state_roped, worldPos);

        protected override void OnInitialize(out IState entrySubState, List<IStateBase> subStates)
        {
            subStates.Add(m_state_walking);
            subStates.Add(m_state_roped);
            subStates.Add(m_state_knockedOut);
            entrySubState = m_state_walking;
        }
        
        void IState.Enter() { }

        void IUpdate.Update(float deltaTime)
        {
            var flipTarget = actor.m_orientation > 0f ? 0f : 180f;
            m_flipRotation = Mathf.MoveTowards(m_flipRotation, flipTarget, 720f * deltaTime);
            actor.m_flip.transform.localRotation = Quaternion.Euler(0f, m_flipRotation, 0f);
        }
        
        void IAttack.Attack()
        {
            if(actor.HasNeedle && (Time.time - m_lastAttackTime) > actor.m_attackCooldown)
            {
                m_lastAttackTime = Time.time;
                actor.m_animator.Trigger(s_idAttack);
                
                actor.Invoke(nameof(ApplyAttackDamage), 0.25f);
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

                actor.HandleWalking(actor.m_walkSpeed, actor.m_walkAcceleration, actor.m_walkDecceleration, actor.m_maxAirSpeed, actor.m_airAcceleration);
                actor.HandleJumping();
            }
            
            void IUpdate.Update(float deltaTime)
            {
                float playWalk = actor.IsGrounded() ? Mathf.Abs(actor.m_rigidbody.velocity.x) : 0f;
                actor.m_animator.SetValue(s_idWalkSpeed, playWalk);
                
                if(Mathf.Abs(actor.NavigationInput) > 0.01f)
                {
                    actor.m_orientation = Mathf.Sign(actor.NavigationInput);
                    actor.m_animator.SetValue(s_idGoingDirection, actor.m_orientation);
                }

            }

            protected override void OnExit()
            {
                base.OnExit();
                actor.m_animator.SetValue(s_idWalkSpeed, 0f);
            }
        }
        
        class State_Roped : SimpleState<Avatar, State_Root>, IState<Vector2>, ITick, IUpdate,IThrowRope
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
                actor.m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                actor.SetPhysMaterial(actor.m_walkMaterial);
                actor.m_animator.SetValue(s_idRoped, true);
            }

            protected override void OnExit()
            {
                actor.m_animator.SetValue(s_idRoped, false);
                if(m_rope)
                    Destroy(m_rope.gameObject);
                base.OnExit();
            }

            void ITick.Tick(float deltaTime)
            {
                actor.HandleWalking(actor.m_walkSpeed, actor.m_walkAcceleration, actor.m_walkDecceleration, actor.m_maxRopeAirSpeed, actor.m_ropeAirAcceleration);
                actor.HandleJumping();

                var ropeDirection = m_rope.GetRopeDirection();
                float targetRotation = Mathf.Atan2(ropeDirection.y, ropeDirection.x) * Mathf.Rad2Deg - 90f;
                actor.m_rigidbody.rotation = Mathf.MoveTowardsAngle(actor.m_rigidbody.rotation, targetRotation, Time.deltaTime * 360f);
                
                // if(actor.RollInput.Value)
                // {
                //     TransitTo(parent.m_state_rolling);
                // }
            }

            void IUpdate.Update(float deltaTime)
            {
        
            }

            void IThrowRope.ThrowRope(Vector2 worldPosition)
            {
                TransitTo(parent.m_state_walking);
            }
        }
        
        class State_KnockedOut : SimpleState<Avatar, State_Root>, IState<float>, ITick , IAttack
        {
            private float m_timeleft;
            
            void IState<float>.Enter(float duration)
            {
                m_timeleft = duration;
            }

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
                // Vector2 force = new()
                // {
                //     x = actor.NavigationInput * actor.m_rollForce,
                //     y = 0
                // };
                // actor.m_rigidbody.AddForce(force, ForceMode2D.Force);

                //actor.HandleJumping();

                m_timeleft -= deltaTime;
                if(m_timeleft <= 0f)
                {
                    TransitTo(parent.m_state_walking);
                }
            }
            
            void IAttack.Attack()
            {
            }
        }
        void IReceivePunch.ReceivePunch(Actor inflictor, Vector2 velocity, float knockoutDuration)
        {
            actor.m_rigidbody.velocity = velocity;
            TransitTo(m_state_knockedOut, knockoutDuration);
            actor.PlayHitEffect();
        }
    }
    
    public void ReceivePunch(Actor from, Vector2 velocity, float knockoutDuration)
    {
        m_machine.SendMessage(msg_receivePunch, (from, velocity, knockoutDuration));
    }
    
    private void PlayHitEffect()
    {
        Debug.Log("Outch! TODO play hit anim", gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if(!m_jumpQueued)
            return;

        m_jumpQueued = false;

        Vector2 normal = Vector2.zero;
        foreach (ContactPoint2D contactPoint2D in col.contacts)
        {
            normal += contactPoint2D.normal;
        }
        
        normal.Normalize();
        m_rigidbody.velocity += normal * m_wallJumpSpeed;
        
        m_animator.Trigger(s_idJump);
    }

    private void ApplyAttackDamage()
    {
        using (HashSetPool<Actor>.Get(out var result))
        {
            ColliderOverlapActors(m_attackArea, new ContactFilter2D(), result);
            foreach (var hitActor in result)
            {
                if(hitActor is INeedleDamageReceiver receiver)
                    receiver.ReceiveNeedleDamage(this);
            }
        }
    }

}
