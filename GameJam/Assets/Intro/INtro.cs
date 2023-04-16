using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class INtro : MonoBehaviour
{
    [SerializeField]
    private SceneReference m_levelScene;
    
    [SerializeField]
    private PlayableDirector m_director;

    private void Awake()
    {
        m_director.stopped += Stopped;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Stopped(null);
        }
    }
    
    private void Stopped(PlayableDirector obj)
    {
        SceneManager.LoadScene(m_levelScene.Path);
    }
}
