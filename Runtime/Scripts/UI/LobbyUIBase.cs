using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;

/// <summary>
/// Base UI class that can be inherited to make more custom UI interactions.
/// </summary>
public class LobbyUIBase : MonoBehaviour
{
    //Main variables
    [SerializeField] TMP_Text lobbyName;
    [SerializeField] TMP_Text playerCountDisplay;

    //Player variables
    [SerializeField] Transform listedPlayersParent;
    [SerializeField] ListedPlayerUI listedPlayerPrefab;
    List<ListedPlayerUI> listedPlayers = new List<ListedPlayerUI>();

    #region Displaying lobby data
    /// <summary>
    /// Absolute basic way of displaying lobby data; only the name.
    /// </summary>
    /// <param name="name"></param>
    public virtual void DisplayLobbyData(string name)
    {
        //Set lobby name
        lobbyName.text = name;
    }

    /// <summary>
    /// Displays lobby data including name and player count with max players.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="playerCount"></param>
    /// <param name="maxPlayers"></param>
    public virtual void DisplayLobbyData(string name, int playerCount, int maxPlayers)
    {
        //Set lobby name
        lobbyName.text = name;

        //Set player count
        playerCountDisplay.text = $"{playerCount}/{maxPlayers}";
    }

    public virtual void DisplayLobbyData(string name, int playerCount, int maxPlayers, Player[] players)
    {
        //Set lobby name
        lobbyName.text = name;

        //Set player count
        playerCountDisplay.text = $"{playerCount}/{maxPlayers}";

        //List the players
        ClearUIElements();
        foreach (Player p in players)
        {
            ListPlayer(p);
        }
    }
    #endregion

    /// <summary>
    /// Clear all the listed players.
    /// </summary>
    void ClearUIElements()
    {
        foreach(ListedPlayerUI p in listedPlayers)
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
