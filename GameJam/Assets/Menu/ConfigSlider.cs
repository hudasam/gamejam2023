using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof( Slider ))]
public class ConfigSlider : MonoBehaviour
{
    [SerializeField] private FloatConfigValue m_configValue;
    private Slider m_slider;

    protected void Awake()
    {
        m_slider = GetComponent<Slider>();
        m_slider.onValueChanged.AddListener(ValueChanged);
        m_configValue.onValueChanged += ValueChanged;
        m_slider.SetValueWithoutNotify(m_configValue.Value);
    }

    protected void OnDestroy()
    {
        m_slider.onValueChanged.RemoveListener(ValueChanged);
        m_configValue.onValueChanged -= ValueChanged;
    }

    private void ValueChanged(float newValue)
    {
        m_configValue.Value = newValue;
        m_slider.SetValueWithoutNotify(newValue);
    }
}