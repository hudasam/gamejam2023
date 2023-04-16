using System;
using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneReference m_introScene;
    
    [SerializeField] private Button m_quitButton;
    // Start is called before the first frame update
    [Preserve]
    public void StartGame()
    {
        SceneManager.LoadScene(m_introScene.Path);
    }
    
    [Preserve]
    public void QuitGame()
    {
        if(!CanQuit())
            return;
        Application.Quit();
    }
    private static bool CanQuit()
    {

        if(Application.platform is RuntimePlatform.WebGLPlayer)
            return false;

        return true;
    }

    private void Start()
    {
        m_quitButton.interactable = CanQuit();
    }
}
