using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoLobbyUI : MonoBehaviour
{
    [SerializeField] TMP_Text lobbyName;

    public void DisplayLobbyData(string name)
    {
        lobbyName.text = name;  
    }
}
