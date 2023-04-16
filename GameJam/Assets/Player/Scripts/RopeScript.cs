using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;

public class RopeScript : MonoBehaviour
{
    public Vector2 anchorDest { get; set; }
    [SerializeField] private float anchorSpeed;
    [SerializeField] private float segmentSize;
    [SerializeField] GameObject nodePrefab;
    private GameObject player;
    private GameObject prevNodeRef;
    private bool ropeSegmented;

    private List<Transform> nodePositions = new List<Transform>();
    private LineRenderer lineRenderer;

    public Vector2 GetRopeDirection(bool endPoint)
    {
        if(nodePositions.Count < 2)
            return Vector2.zero;

        var firstIndex = endPoint ? ^2 : 0;
        return (nodePositions[firstIndex].position.xy() - nodePositions[^1].position.xy()).normalized;
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        prevNodeRef = gameObject;
        ropeSegmented = false;
        lineRenderer = GetComponent<LineRenderer>();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((Vector2)transform.position != anchorDest) {
            transform.position = Vector2.MoveTowards(transform.position, anchorDest, anchorSpeed);
        }
        else if (!ropeSegmented)
        {
            nodePositions.Add(transform);
            SegmentRope();
            prevNodeRef.GetComponent<Joint2D>().connectedBody = player.GetComponent<Rigidbody2D>();
            ropeSegmented = true;
        }
        renderRope();
        
    }

    void SegmentRope()
    {
        while (NodeDistance(prevNodeRef) > segmentSize)
        {
            CreateNode();
        }
        CreateNode((Vector2)player.transform.position);
    }
    void CreateNode() 
    {
        Vector2 relativeNodePos = (player.transform.position - prevNodeRef.transform.position).normalized;
        relativeNodePos *= segmentSize;
        Vector2 nodePos = (Vector2)prevNodeRef.transform.position + relativeNodePos;
        

        GameObject newNode = (GameObject)Instantiate(nodePrefab, nodePos, Quaternion.identity);

        newNode.transform.SetParent(transform);
        prevNodeRef.GetComponent<Joint2D>().connectedBody = newNode.GetComponent<Rigidbody2D>();

        nodePositions.Add(newNode.transform);
        prevNodeRef = newNode;

    }
    void CreateNode(Vector2 position) 
    {
        GameObject newNode = (GameObject)Instantiate(nodePrefab, position, Quaternion.identity);
        newNode.transform.SetParent(transform);
        prevNodeRef.GetComponent<Joint2D>().connectedBody = newNode.GetComponent<Rigidbody2D>();
        nodePositions.Add(newNode.transform);
        prevNodeRef = newNode;
    }

    float NodeDistance(GameObject node) 
    {
        return Vector2.Distance(player.transform.position, prevNodeRef.transform.position);
    }
    void renderRope()
    {
        lineRenderer.positionCount = nodePositions.Count;

        for (int i = 0;i<nodePositions.Count;i++) {
            lineRenderer.SetPosition(i, nodePositions[i].transform.position);
        }
        //lineRenderer.SetPosition(nodeCount,player.transform.position);

    }




}
