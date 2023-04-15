using System.Collections;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils;
using UnityEngine;

[CreateAssetMenu(fileName = "GJ/GlobalSettings")]
public class GlobalSettings : SingletonAsset<GlobalSettings>
{
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private LayerMask m_playerMask;
    
    public LayerMask GroundMask => m_groundMask;
    public LayerMask PlayerMask => m_playerMask;
}
