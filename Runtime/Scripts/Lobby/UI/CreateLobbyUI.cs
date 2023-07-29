using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vincent.LobbySystem;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_InputField lobbyName;

    public void OnStart()
    {
        TogglePanel();
    }

    public void TryCreateLobby()
    {
        LobbyManager.Instance.CreateLobby(lobbyName.text);
        TogglePanel();
    }

    public void TogglePanel()
    {
        if (panel.activeInHierarchy)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    }
}
