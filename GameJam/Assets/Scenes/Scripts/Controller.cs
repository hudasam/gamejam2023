using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private CameraController m_cameraController;
    [SerializeField] private Avatar m_avatar;

    [SerializeField] private TMP_Text m_availableActionIndicator;

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
            OnAvatarActionChanged(m_avatar ? m_avatar.AvailableAction.Value : null);
        }
    }
    
    void Awake()
    {
        // an ugly workaround to initialize the serialized property
        var avatar = Avatar;
        Avatar = null;
        Avatar = avatar;
    }

    private void OnAvatarActionChanged(PlayerAction obj)
    {
        if(obj != null)
        {
            m_availableActionIndicator.gameObject.SetActive(true);
            m_availableActionIndicator.text = obj.Description;
        }
        else
        {
            m_availableActionIndicator.gameObject.SetActive(false);
        }
    }

}
