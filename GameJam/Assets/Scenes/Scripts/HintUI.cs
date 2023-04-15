using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HintUI : MonoBehaviour
{
    private Animator m_animator;
    private readonly Dictionary<PlayerHint, (int count, Image image)> m_hints = new();
    [SerializeField] private Image m_hintTemplate;
    
    private static readonly AnimatorBool s_idShowBubble = "Show";
    
    void Awake()
    {
        m_hintTemplate.gameObject.SetActive(false);
        m_animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        m_animator.SetValue(s_idShowBubble, m_hints.Count > 0);
    }

    public void AddHint(PlayerHint hint)
    {
        if(m_hints.TryGetValue(hint, out var hintInfo))
        {
            hintInfo.count++;
            m_hints[hint] = hintInfo;
            return;
        }
        
        var hintUI = Instantiate(m_hintTemplate, m_hintTemplate.transform.parent);
        hintUI.sprite = hint.Icon;
        m_hints.Add(hint, (1, hintUI));
        hintUI.gameObject.SetActive(true);
    }
    
    public void RemoveHint(PlayerHint hint)
    {
        if(!m_hints.TryGetValue(hint, out var hintInfo))
            return;
        
        
        hintInfo.count--;
        if(hintInfo.count <= 0)
        {
            m_hints.Remove(hint);
            Destroy(hintInfo.image.gameObject);
        }
        else
        {
            m_hints[hint] = hintInfo;
        }

    }
}
