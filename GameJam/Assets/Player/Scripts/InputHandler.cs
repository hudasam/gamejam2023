using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    //InputHandler for player

    public Vector2 inputVector2D { get; private set; }
    public bool inputJump { get; private set; }


    // Update is called once per frame
    void Update()
    {
        inputVector2D = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        inputJump = Input.GetKeyDown(KeyCode.Space);
    }
}
