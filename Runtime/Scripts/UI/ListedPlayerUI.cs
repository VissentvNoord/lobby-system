using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListedPlayerUI : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;

    public void DisplayPlayerData(string name)
    {
        playerName.text = name;
    }
}
