using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventHandler : MonoBehaviour
{
    public AuthEvents auth;
    public LobbyEvents lobbies;
    public RelayEvents relay;

    private void Start()
    {
        auth.onFetchedPlayerName.AddListener(delegate { Debug.Log("Got random name"); });
        auth.onAuthenticationFail.AddListener(delegate { Debug.LogError("Authentication failed"); });
        auth.onAuthenticated.AddListener(delegate { Debug.Log("Authentication succeeded"); });
    }
}

[System.Serializable]
public struct AuthEvents
{
    public UnityEvent onAuthenticated;
    public UnityEvent onAuthenticationFail;
    public UnityEvent onFetchedPlayerName;
}

[System.Serializable]
public struct LobbyEvents
{
    public UnityEvent onLobbyCreated;
    public UnityEvent onLobbyJoined;
    public UnityEvent onLobbyDeleted;
    public UnityEvent onLobbyLeave;
}

[System.Serializable]
public struct RelayEvents
{
    public UnityEvent onStartGame;
    public UnityEvent onStartGameFail;
    public UnityEvent onJoinGame;
    public UnityEvent onJoinGameFail;
}
