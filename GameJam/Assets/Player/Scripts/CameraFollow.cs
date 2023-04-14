using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject player;

    [SerializeField] float speed;
    [SerializeField] Vector2 offset;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 targetPos = new Vector2(player.transform.position.x,player.transform.position.y) + offset;
        transform.position = Vector2.Lerp(transform.position, targetPos, speed);
    }
}
