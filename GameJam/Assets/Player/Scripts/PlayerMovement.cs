using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Rigidbody2D rb;

    private InputHandler inputHandler;

    [SerializeField] private float groundProximity;
    [SerializeField] private int groundMask;

    void Awake() 
    {
        inputHandler = GetComponent<InputHandler>();
        groundMask = LayerMask.GetMask("Ground");
        groundProximity = groundProximity + transform.localScale.y / 2;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
        isGrounded();
    }

    void Move() {
        Vector2 change = inputHandler.inputVector2D.normalized;
        change *= speed * Time.deltaTime;
        transform.Translate(change.x, 0, 0);
    }

    void Jump() {
        if (inputHandler.inputJump && isGrounded()) {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    bool isGrounded() 
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, groundProximity, groundMask);
        Debug.DrawRay(transform.position, -transform.up,Color.green);
        if (hit) {
            return true;
        }
        return false;
    }
}
