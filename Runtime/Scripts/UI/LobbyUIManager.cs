using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Vincent.LobbySystem.UI;

public class LobbyUIManager : MonoBehaviour
{
    LobbyUIBase currentLobbyUI;

    [SerializeField] TMP_Text currentLobbyCode;

    [SerializeField] Transform listingParent;
    [SerializeField] ListedLobbyUI listedLobbyPrefab;

    List<ListedLobbyUI> lobbyList = new List<ListedLobbyUI>();

    private void Start()
    {
        currentLobbyUI = FindAnyObjectByType<LobbyUIBase>();
        currentLobbyUI.OnStart();
    }

    public void ClearList()
    {
        foreach(ListedLobbyUI lobby in lobbyList)
        {
            Destroy(lobby.gameObject);
        }

        lobbyList.Clear();  
    }

    public void ListLobby(string lobbyID, string lobbyName, int playerCount, int maxPlayers)
    {
        ListedLobbyUI newLobbyUI = Instantiate(listedLobbyPrefab, listingParent);
        newLobbyUI.DisplayLobbyData(lobbyID, lobbyName, playerCount, maxPlayers);

        lobbyList.Add(newLobbyUI); 
    }

    public void SetCurrentLobbyCode(string code)
    {
        currentLobbyCode.text = code;
    }

    public void DisplayLobby(Lobby lobby, bool isHost = false)
    {
        //Checking if null; return error.
        if(currentLobbyUI == null)
        {
            Debug.LogError("Trying to display lobby UI, but no lobby UI exists in the scene!");
            return;
        }

        currentLobbyUI.DisplayLobbyData(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Players.ToArray(), isHost);
    }
}
