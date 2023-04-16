using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneReference m_introScene;
    
    [SerializeField] private GameObject m_quitButton;
    // Start is called before the first frame update
    [Preserve]
    public void StartGame()
    {
        SceneManager.LoadScene(m_introScene.Path);
    }
    
    [Preserve]
    public void QuitGame()
    {
        Application.Quit();
    }

    private void Start()
    {
        
    }
}
