using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUpdater
{
    LobbyManager manager;
    float heartBeatTimer;
    float lobbyUpdateTimer;

    public void OnStart(LobbyManager _manager)
    {
        manager = _manager;
    }

    //Handle updates; call this in lobby manager(in update)
    public void HandleUpdates()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    async void HandleLobbyHeartBeat()
    {
        //Getting hosted lobby; if null return.
        if (manager.HostLobby() == null)
            return;

        Lobby hostLobby = manager.HostLobby();

        heartBeatTimer -= Time.deltaTime;
        if (heartBeatTimer < 0)
        {
            float heartBeatTimerMax = 15;
            heartBeatTimer = heartBeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    async void HandleLobbyPollForUpdates()
    {
        //Getting joined lobby; if null return.
        if (manager.JoinedLobby() == null)
            return;

        Lobby joinedLobby = manager.JoinedLobby();

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            float updateTimerMax = 1.1f;
            lobbyUpdateTimer = updateTimerMax;

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

            //Resetting joined lobby to recache.
            manager.SetJoinedLobby(lobby);

            //Updating the lobby
            manager.UpdateLobby();

            //Debug.Log($"Updated joined lobby {lobby.Name} at {Time.realtimeSinceStartup}");
        }
    }
}
