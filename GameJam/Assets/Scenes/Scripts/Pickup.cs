using System.Collections.Generic;
using SeweralIdeas.Utils;
using UnityEngine;

[RequireComponent(typeof(Zone))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private PlayerAction m_playerAction;
    
    private Zone m_zone;
    private Dictionary<Avatar, MultiControl<PlayerAction>.Request> m_actionRequests = new();


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
                if(m_playerAction == null)
                    Debug.LogError($"{nameof(m_playerAction)} is null", gameObject);
                var request = avatar.AvailableAction.CreateRequest(name, (int)Avatar.ActionPriority.Pickup, m_playerAction, true);
                m_actionRequests[avatar] = request;
            }
            else
            {
                var request = m_actionRequests[avatar];
                request.Dispose();
                m_actionRequests.Remove(avatar);
            }
        }
    }
}
