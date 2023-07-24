using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventHandler : MonoBehaviour
{
    public AuthEvents auth;
    public LobbyEvents lobbies;

    private void Start()
    {
        auth.onRandomName.AddListener(delegate { Debug.Log("Got random name"); });
        auth.onAuthenticationFail.AddListener(delegate { Debug.LogError("Authentication failed"); });
        auth.onAuthenticated.AddListener(delegate { Debug.Log("Authentication succeeded"); });
    }
}

[System.Serializable]
public struct AuthEvents
{
    public UnityEvent onAuthenticated;
    public UnityEvent onAuthenticationFail;
    public UnityEvent onRandomName;
}

[System.Serializable]
public struct LobbyEvents
{
    public UnityEvent onLobbyCreated;
    public UnityEvent onLobbyJoined;
    public UnityEvent onLobbyDeleted;
    public UnityEvent onLobbyLeave;
}
