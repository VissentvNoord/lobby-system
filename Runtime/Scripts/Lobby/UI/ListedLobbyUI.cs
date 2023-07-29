using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vincent.LobbySystem;

public class ListedLobbyUI : MonoBehaviour
{
    [SerializeField] string lobbyID;

    [SerializeField] TMP_Text lobbyName;
    [SerializeField] TMP_Text playerCount;

    public void DisplayLobbyData(string id, string name, int pCount, int maxPlayers)
    {
        lobbyID = id;
        lobbyName.text = name;
        playerCount.text = $"{pCount}/{maxPlayers}";
    }

    public void OnJoinClick()
    {
        LobbyManager.Instance.JoinLobbyByID(lobbyID);
    }
}
