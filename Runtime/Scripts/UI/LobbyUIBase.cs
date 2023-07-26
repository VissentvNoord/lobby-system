using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

/// <summary>
/// Base UI class that can be inherited to make more custom UI interactions.
/// </summary>

namespace Vincent.LobbySystem.UI
{
    public class LobbyUIBase : MonoBehaviour
    {
        //Main variables
        [Header("Lobby variables")]
        [SerializeField] TMP_Text lobbyName;
        [SerializeField] TMP_Text playerCountDisplay;
        [SerializeField] Button startGameButton;

        //Player variables
        [Header("Player variables")]
        [SerializeField] Transform listedPlayersParent;
        [SerializeField] ListedPlayerUI listedPlayerPrefab;
        List<ListedPlayerUI> listedPlayers = new List<ListedPlayerUI>();

        /// <summary>
        /// Start method called from the UI manager.
        /// </summary>
        public virtual void OnStart()
        {
            startGameButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        #region Displaying lobby data
        /// <summary>
        /// Absolute basic way of displaying lobby data; only the name.
        /// </summary>
        /// <param name="name"></param>
        public virtual void DisplayLobbyData(string name, bool isHost)
        {
            lobbyName.text = name;

            if (isHost)
            {
                PlayerIsHost();
            }
        }

        /// <summary>
        /// Displays lobby data including name and player count with max players.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playerCount"></param>
        /// <param name="maxPlayers"></param>
        public virtual void DisplayLobbyData(string name, int playerCount, int maxPlayers, bool isHost)
        {
            DisplayLobbyData(name, isHost);

            //Set player count
            playerCountDisplay.text = $"{playerCount}/{maxPlayers}";
        }

        public virtual void DisplayLobbyData(string name, int playerCount, int maxPlayers, Player[] players, bool isHost)
        {
            DisplayLobbyData(name, isHost);

            playerCountDisplay.text = $"{playerCount}/{maxPlayers}";

            //List the players
            ClearUIElements();
            foreach (Player p in players)
            {
                ListPlayer(p);
            }
        }

        /// <summary>
        /// Used to enable UI elements only visible to the host of the lobby.
        /// </summary>
        protected virtual void PlayerIsHost()
        {
            startGameButton.gameObject.SetActive(true);
        }
        #endregion

        /// <summary>
        /// Clear all the listed players.
        /// </summary>
        void ClearUIElements()
        {
            foreach (ListedPlayerUI p in listedPlayers)
            {
                Destroy(p.gameObject);
            }

            listedPlayers.Clear();
        }

        /// <summary>
        /// Used to list a player. Creates a new GameObject in the UI system.
        /// </summary>
        /// <param name="player"></param>
        protected virtual void ListPlayer(Player player)
        {
            string playerName = player.Data["PlayerName"].Value;

            ListedPlayerUI listedPlayer = Instantiate(listedPlayerPrefab, listedPlayersParent);
            listedPlayer.DisplayPlayerData(playerName);

            listedPlayers.Add(listedPlayer);
        }
    }
}