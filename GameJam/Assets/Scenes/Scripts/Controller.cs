using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Controller : SingletonBehaviour<Controller>
{
    [SerializeField] private CameraController m_cameraController;
    [SerializeField] private Avatar m_avatar;

    [SerializeField] private TMP_Text m_availableActionIndicator;

    [SerializeField] private KeyCode m_leftButton;
    [SerializeField] private KeyCode m_rightButton;
    [SerializeField] private KeyCode m_jumpButton;
    [SerializeField] private KeyCode m_contextButton;
    [SerializeField] private KeyCode m_attackButton;
    
    public Avatar Avatar
    {
        get => m_avatar;
        set 
        {
            if(m_avatar == value)
                return;

            if(m_avatar != null)
            {
                m_avatar.AvailableAction.ValueChanged -= OnAvatarActionChanged;
            }
            m_avatar = value;
            if(m_avatar != null)
            {
                m_avatar.AvailableAction.ValueChanged += OnAvatarActionChanged;
            }

            var t = m_avatar ? m_avatar.AvailableAction.Value : (null, null);
            OnAvatarActionChanged(t);
            m_cameraController.SetTarget(m_avatar ? m_avatar.transform : null);
        }
    }
    
    void Awake()
    {
        // an ugly workaround to initialize the serialized property
        var avatar = Avatar;
        Avatar = null;
        Avatar = avatar;
    }
    
    private void OnAvatarActionChanged((PlayerAction obj,Transform transform) tup)
    {
        if(tup.obj != null)
        {
            m_availableActionIndicator.gameObject.SetActive(true);
            m_availableActionIndicator.text = tup.obj.Description;
        }
        else
        {
            m_availableActionIndicator.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if(Avatar)
        {

            Avatar.NavigationInput = (Input.GetKey(m_leftButton) ? -1 : 0) + (Input.GetKey(m_rightButton) ? 1 : 0);
            Avatar.JumpInput.Value = Input.GetKey(m_jumpButton);
            Avatar.AttackInput.Value = Input.GetKey(m_attackButton);
            Avatar.ContextInput.Value = Input.GetKey(m_contextButton);
        }
    }
}
