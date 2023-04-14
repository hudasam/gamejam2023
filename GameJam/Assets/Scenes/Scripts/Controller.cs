using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private CameraController m_cameraController;
    [SerializeField] private Avatar m_avatar;

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

    private void OnAvatarActionChanged(Action obj)
    {
        Debug.Log($"Action changed to {obj}");
    }

}
