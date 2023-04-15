using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GJ/Action")]
public class PlayerAction : ScriptableObject
{
    [SerializeField] private string m_description;
    [SerializeField] private ActionType m_type;
    public enum ActionType
    { 
        Collect,
        Swing,
        Use
    }
    public string Description => m_description;
    public ActionType  Type=> m_type;
}