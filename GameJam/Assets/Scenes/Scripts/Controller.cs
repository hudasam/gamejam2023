using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class Controller : SingletonBehaviour<Controller>
{
    [SerializeField]
    private SceneReference m_menuScene;
    
    [SerializeField] private CameraController m_cameraController;
    [SerializeField] private Avatar m_avatar;

    [SerializeField] private TMP_Text m_availableActionIndicator;
    [SerializeField] private string m_availableActionIndicatorFormat = "Press {0} to {1}";
    [SerializeField] private KeyCode m_leftButton;
    [SerializeField] private KeyCode m_rightButton;
    [SerializeField] private KeyCode m_jumpButton;
    [SerializeField] private KeyCode m_contextButton;
    [SerializeField] private KeyCode m_attackButton;
    [SerializeField] private TMP_Text m_mothIndicator;

    [SerializeField] private Animator m_chapterAnimator;
    private static readonly AnimatorTrigger s_chapterTrigger = "Trigger";
    [SerializeField] private TMP_Text m_chapterText;

    [SerializeField] private GameObject m_pauseMenu;
    
    [Preserve]
    public void ExitToMenu()
    {
        SceneManager.LoadScene(m_menuScene.Path);
    }
    
    private int m_mothCount;

    [Preserve]
    public void DisplayChapter(string chapterName)
    {
        m_chapterText.text = chapterName;
        m_chapterAnimator.Trigger(s_chapterTrigger);
    }
    
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
        Time.timeScale = 1;
        // an ugly workaround to initialize the serialized property
        var avatar = Avatar;
        Avatar = null;
        Avatar = avatar;
        m_mothIndicator.transform.parent.gameObject.SetActive(false);
        
        m_pauseMenu.SetActive(false);
    }

    protected override void OnDestroyed()
    {
        Time.timeScale = 1;
        base.OnDestroyed();
    }
    
    private void OnAvatarActionChanged((PlayerAction obj,Transform transform) tup)
    {
        if(tup.obj != null)
        {
            m_availableActionIndicator.gameObject.SetActive(true);
            m_availableActionIndicator.text = string.Format(m_availableActionIndicatorFormat, m_contextButton.ToString(), tup.obj.Description);
        }
        else
        {
            m_availableActionIndicator.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if(Avatar && !m_pauseMenu.activeSelf)
        {

            Avatar.NavigationInput = (Input.GetKey(m_leftButton) ? -1 : 0) + (Input.GetKey(m_rightButton) ? 1 : 0);
            Avatar.JumpInput.Value = Input.GetKey(m_jumpButton);
            Avatar.AttackInput.Value = Input.GetKey(m_attackButton);
            Avatar.ContextInput.Value = Input.GetKey(m_contextButton);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    [Preserve]
    public void TogglePause()
    {
        m_pauseMenu.SetActive(!m_pauseMenu.activeSelf);
        Time.timeScale = m_pauseMenu.activeSelf ? 0f : 1f;
    }

    public void AddMoth()
    {
        m_mothCount++;
        m_mothIndicator.text = m_mothCount.ToString();
        m_mothIndicator.transform.parent.gameObject.SetActive(true);
    }
}
