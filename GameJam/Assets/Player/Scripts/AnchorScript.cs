using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorScript : MonoBehaviour
{
    [SerializeField] private float maxDistance;
    [SerializeField] private int GroundMask;

    [SerializeField] GameObject anchor;

    GameObject rope;
    private bool roped;
    private void Awake()
    {
        GroundMask = LayerMask.GetMask("Ground");
        rope = null;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NoPlayerCollision"), LayerMask.NameToLayer("Player"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("NoPlayerCollision"), LayerMask.NameToLayer("NoPlayerCollision"));
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && rope == null) {
            // TODO change
            
            ThrowAnchor(out rope);
        }
        if (Input.GetMouseButtonDown(1) && rope!=null) {
            Destroy(rope);
            rope = null;
        }
    }

    void ThrowAnchor(out GameObject rope) 
    {
        if (getDestination(out Vector2 anchorDest))
        {
            rope = (GameObject)Instantiate(anchor, transform.position, Quaternion.identity);
            RopeScript rs = rope.GetComponent<RopeScript>();
            rs.anchorDest = anchorDest;
        }
        else {
            rope = null;
        }
    }

    bool getDestination(out Vector2 anchorDest) 
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        Vector2 targetDir = (mousePos - playerPos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(playerPos, targetDir, maxDistance, GroundMask);
        Debug.DrawRay(transform.position, targetDir, Color.green);
        if (hit.collider!=null ) {
            anchorDest = hit.point;
            return true;
        }
        anchorDest = Vector2.zero;
        return false;
    }
    public bool isRoped() {
        if (rope == null) return false;
        return true;
    }
}
