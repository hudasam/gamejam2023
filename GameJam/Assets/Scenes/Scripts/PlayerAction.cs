using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GJ/Action")]
public class PlayerAction : ScriptableObject
{
    [SerializeField] private string m_description;
    public string Description => m_description;
}