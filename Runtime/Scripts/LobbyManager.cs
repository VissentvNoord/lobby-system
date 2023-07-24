using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine;
using QFSW.QC;
using System.Threading.Tasks;

[RequireComponent(typeof(EventHandler))]
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    LobbyUpdater lobbyUpdater;
    LobbyUIManager UI;
    EventHandler events;

    Lobby hostLobby;
    Lobby joinedLobby;
    string playerName;

    private void Awake()
    {
        Instance = this;

        lobbyUpdater = new LobbyUpdater();
        lobbyUpdater.OnStart(this);

        UI = GetComponent<LobbyUIManager>();
        events = GetComponent<EventHandler>();

        events.lobbies.onLobbyLeave.AddListener(ClearLobbyData);
    }

    private void Start()
    {
        playerName = "Player" + Random.Range(10, 99);
        events.auth.onRandomName.Invoke();
        Debug.Log(playerName);
    }

    public async void Authenticate()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                events.auth.onAuthenticated.Invoke();
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException e)
        {
            events.auth.onAuthenticationFail.Invoke();
            Debug.Log(e);
        }
    }

    private void Update()
    {
        lobbyUpdater.HandleUpdates();
    }

    #region Getting and setting cached lobbies
    public Lobby HostLobby()
    {
        return hostLobby;
    }

    public Lobby JoinedLobby()
    {
        return joinedLobby;
    }

    public void SetHostLobby(Lobby newLobby)
    {
        hostLobby = newLobby;
    }

    public void SetJoinedLobby(Lobby newLobby)
    {
        joinedLobby = newLobby;
    }

    public void UpdateLobby()
    {
        UI.DisplayLobby(joinedLobby);
    }
    #endregion

    #region Creating and joining lobbies
    //Create lobby
    [Command]
    public async void CreateLobby(string lobbyName = "MyLobby")
    {
        try
        {
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),

                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Party") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);

            UI.DisplayLobby(joinedLobby);

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

            events.lobbies.onLobbyCreated.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //Join lobby with code
    [Command]
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = _joinedLobby;

            UI.DisplayLobby(joinedLobby);

            Debug.Log("Joined lobby with code: " + lobbyCode);

            events.lobbies.onLobbyJoined.Invoke();

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByID(string lobbyID)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIDOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, joinLobbyByIDOptions);

            joinedLobby = _joinedLobby;

            //UI.SetLobbyName(joinedLobby.Name);

            Debug.Log("Joined lobby with id: " + lobbyID);

            UI.SetCurrentLobbyCode(joinedLobby.LobbyCode);
            UI.DisplayLobby(joinedLobby);

            events.lobbies.onLobbyJoined.Invoke();

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //Quick join lobby
    [Command]
    async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log($"Quick joined lobby: {lobby.LobbyCode}");
            joinedLobby = lobby;

            //UI.SetLobbyName(joinedLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
            }
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            events.lobbies.onLobbyLeave.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    void ClearLobbyData()
    {
        SetHostLobby(null);
        SetJoinedLobby(null);
    }
    #endregion

    #region Player handling
    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in lobby: {lobby.Name} {lobby.Data["GameMode"].Value}");
        foreach (Player player in lobby.Players)
        {
            Debug.Log($"{player.Id} {player.Data["PlayerName"].Value}");
        }
    }

    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }

    async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> {
            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    async void MigrateHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void SetLocalPlayerName(string newName)
    {
        playerName = newName;
    }

    public string GetLocalPlayerName()
    {
        return playerName;
    }

    #endregion

    #region Listing lobbies
    [Command]
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            UI.ClearList();

            foreach (Lobby lobby in queryResponse.Results)
            {
                UI.ListLobby(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void OnApplicationQuit()
    {
        if (joinedLobby == null)
            return;

        LeaveLobby();
    }
    #endregion
}
