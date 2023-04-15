using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    [SerializeField] private Transform[] m_teleports;

    [System.Serializable]
    private struct Save
    {
        [SerializeField] public Vector2 m_quickSavePos;
        [SerializeField] public Vector2 m_quickSaveVel;
    }

    private Save m_save = new Save();
    
    void Update()
    {
        var avatar = Controller.GetInstance().Avatar;
        #if UNITY_EDITOR
        for( int i = (int)KeyCode.F1; i < (int)KeyCode.F15; ++i )
        {
            if(i >= m_teleports.Length)
                break;
            
            KeyCode key = (KeyCode)i;
            if(Input.GetKeyDown(key))
            {
                var dest = m_teleports[i];
                if(dest != null)
                {
                    // teleport to
                    avatar.transform.position = dest.position.xy();
                    avatar.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            var rb = avatar.GetComponent<Rigidbody2D>();
            m_save.m_quickSavePos = Controller.GetInstance().Avatar.transform.position;
            m_save.m_quickSaveVel = rb.velocity;

            string json = JsonUtility.ToJson(m_save);
            PlayerPrefs.SetString(SavePrefsKey, json);
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            var json = PlayerPrefs.GetString(SavePrefsKey);
            m_save = JsonUtility.FromJson<Save>(json);
            
            var rb = avatar.GetComponent<Rigidbody2D>();
            avatar.transform.position = m_save.m_quickSavePos;
            rb.velocity = m_save.m_quickSaveVel;
        }
        #endif
    }
    
    private const string SavePrefsKey = "DEBUG_QUICKSAVE";
}
