using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GJ/Hint")]
public class PlayerHint : ScriptableObject
{
    [SerializeField] private string m_description;
    [SerializeField] private Sprite m_icon;
    public string Description => m_description;
    public Sprite Icon => m_icon;
}