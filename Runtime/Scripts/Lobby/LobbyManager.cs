using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine;
using QFSW.QC;
using System.Threading.Tasks;

namespace Vincent.LobbySystem
{
    [RequireComponent(typeof(EventHandler))]
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance;

        //Components
        LobbyUpdater lobbyUpdater;
        LobbyUIManager UI;
        EventHandler events;

        //Lobby data
        Lobby hostLobby;
        Lobby joinedLobby;

        //Player and session data
        string playerName;
        public const string RELAY_KEY = "RELAY_KEY";
        public const string PLAYER_NAME_KEY = "PlayerName";

        public int PlayerCount { get; private set; }

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
            if (!CheckForSavedName())
            {
                SetLocalPlayerName("Player" + Random.Range(10, 99));
                Debug.Log(playerName);
            }

            events.auth.onFetchedPlayerName.Invoke();
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

        #region Lobby data
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

        /// <summary>
        /// <para>1. Refreshing the lobby data and UI. </para>
        /// <para>2. If Relay key changed, player joins Relay server.</para>
        /// </summary>
        public async void RefreshLobby()
        {
            UI.DisplayLobby(joinedLobby);
            PlayerCount = joinedLobby.Players.Count;

            if (joinedLobby.Data[RELAY_KEY].Value != "0")
            {
                if (HostLobby() != null)
                {
                    Debug.Log("Not joining relay because we are a host.");
                    return;
                }

                bool joinedRelay = await TestRelay.Instance.JoinRelay(joinedLobby.Data[RELAY_KEY].Value);

                if (joinedRelay)
                {
                    events.relay.onJoinGame.Invoke();
                }
                else
                {
                    events.relay.onJoinGameFail.Invoke();
                }

                joinedLobby = null;
            }
        }
        #endregion

        #region Lobby functions
        [Command]
        public virtual async void CreateLobby(string lobbyName = "MyLobby")
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
                        { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                hostLobby = lobby;
                joinedLobby = hostLobby;

                PrintPlayers(hostLobby);

                UI.DisplayLobby(joinedLobby, true);

                Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

                events.lobbies.onLobbyCreated.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public virtual async void JoinLobbyByCode(string lobbyCode)
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

        public virtual async void JoinLobbyByID(string lobbyID)
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

        protected virtual async void QuickJoinLobby()
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

        public virtual async void LeaveLobby()
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


        /// <summary>
        /// List lobbies using logs or UI.
        /// </summary>
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
                    Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
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

        #region Player handling
        private void PrintPlayers()
        {
            PrintPlayers(joinedLobby);
        }

        void PrintPlayers(Lobby lobby)
        {
            Debug.Log($"Players in lobby: {lobby.Name}");
            foreach (Player player in lobby.Players)
            {
                Debug.Log($"{player.Id} {player.Data[PLAYER_NAME_KEY].Value}");
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
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        }

        public string GetLocalPlayerName()
        {
            return playerName;
        }

        bool CheckForSavedName()
        {
            bool hasSavedName = false;

            if (PlayerPrefs.GetString(PLAYER_NAME_KEY) != "")
            {
                hasSavedName = true;

                string savedName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
                SetLocalPlayerName(savedName);
            }

            return hasSavedName;
        }

        #endregion

        #region Relay
        public async void StartGame()
        {
            if (HostLobby() == null)
                return;

            try
            {
                Debug.Log("Starting game");

                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;

                events.relay.onStartGame.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                events.relay.onStartGameFail.Invoke();
            }
        }
    }
    #endregion
}
