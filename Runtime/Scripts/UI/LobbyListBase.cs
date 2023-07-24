using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyListBase : MonoBehaviour
{
    CreateLobbyUI createLobbyUI;

    private void Start()
    {
        createLobbyUI = GetComponentInChildren<CreateLobbyUI>();
        createLobbyUI.OnStart();

        gameObject.SetActive(false);    
    }
}
