using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform m_target;
    private Rigidbody2D m_targetRB;

    [SerializeField] private Transform m_cameraFixture;
    [SerializeField] Vector2 m_smoothTime;
    [SerializeField] Vector2 m_velocityToDistance;
    [SerializeField] private float m_xVelThreshold;
    
    private Vector2 m_velocity;

    private float m_xVelCache;

    public void SetTarget(Transform target)
    {
        m_target = target;
        m_targetRB = target ? target.GetComponent<Rigidbody2D>() : null;
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 targetPos = (Vector2)m_target.position;
        if(m_targetRB && Mathf.Abs(m_targetRB.velocity.x) > m_xVelThreshold)
        {
            m_xVelCache = Mathf.Sign(m_targetRB.velocity.x);
        }
        
        targetPos.y += m_targetRB.velocity.y * m_velocityToDistance.y;
        targetPos.x += m_xVelCache * m_velocityToDistance.x;

        Vector2 pos = m_cameraFixture.position;
        pos.x = Mathf.SmoothDamp(pos.x, targetPos.x, ref m_velocity.x, m_smoothTime.x, float.PositiveInfinity, Time.deltaTime);
        pos.y = Mathf.SmoothDamp(pos.y, targetPos.y, ref m_velocity.y, m_smoothTime.y, float.PositiveInfinity, Time.deltaTime);
        m_cameraFixture.position = pos;
    }
}
