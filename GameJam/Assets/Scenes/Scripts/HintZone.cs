using System.Collections.Generic;
using SeweralIdeas.Utils;
using UnityEngine;

[RequireComponent(typeof(Zone))]
public class HintZone : MonoBehaviour
{
    [SerializeField] private PlayerHint m_hint;
    
    private Zone m_zone;

    protected void Awake()
    {
        m_zone = GetComponent<Zone>();
        m_zone.ActorEnterExit += OnActorEnterExit;
    }

    protected void OnDestroy()
    {
        m_zone.ActorEnterExit -= OnActorEnterExit;
    }
    
    private void OnActorEnterExit(Actor actor, bool entered)
    {
        if(actor is Avatar avatar)
        {
            if(entered)
            {
                avatar.Hints.AddHint(m_hint);
            }
            else
            {
                avatar.Hints.RemoveHint(m_hint);
            }
        }
    }
}
