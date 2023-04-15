using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class WallShadow : MonoBehaviour
{
    [SerializeField] private Vector2 m_offset = new Vector2(0.3f, -0.3f);
    [SerializeField] private bool m_dynamic;
    private SpriteRenderer m_renderer;
    private SpriteRenderer m_parentRenderer;

    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
    }


    private void Start()
    {
        UpdatePosition();
    }
    
    private void Update()
    {
        if(m_dynamic || !Application.isPlaying)
        {
            UpdatePosition();
        }
        else
        {
            enabled = false;
        }
    }
    
    private void UpdatePosition()
    {
        transform.position = transform.parent.position + (Vector3)m_offset;
        m_renderer.size = m_parentRenderer.size;
    }
}
