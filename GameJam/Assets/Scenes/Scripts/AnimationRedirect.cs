using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class AnimationRedirect : MonoBehaviour
{
    [SerializeField] private Material m_targetMaterial;
    [SerializeField] private string m_materialProperty;
    [SerializeField] private string m_materialColorProperty;
    public float Property;
    public Color ColorProperty;

    public UnityEvent<float> Redirect = new();

    private void OnDidApplyAnimationProperties()
    {
        Redirect.Invoke(Property);
        if(m_targetMaterial)
        {
            if(!string.IsNullOrEmpty(m_materialProperty))
                m_targetMaterial.SetFloat(m_materialProperty, Property);
            
            if(!string.IsNullOrEmpty(m_materialColorProperty))
                m_targetMaterial.SetColor(m_materialColorProperty, ColorProperty);
        }
    }
}
