using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services;
using UnityEngine;
using Vincent.LobbySystem;

public class AuthenticateUI : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;

    [SerializeField] GameObject inputNameObj;
    TMP_InputField nameInput;

    private void Start()
    {
        nameInput = inputNameObj.GetComponentInChildren<TMP_InputField>(); 
    }

    void SetPlayerNameUI(string name)
    {
        playerName.text = name; 
    }

    public void SetPlayerName()
    {
        SetPlayerNameUI(LobbyManager.Instance.GetLocalPlayerName());
    }

    public void ConfirmName()
    {
        if(nameInput.text == "")
            return;

        string newName = nameInput.text;    

        //Set the name
        LobbyManager.Instance.SetLocalPlayerName(newName);
        SetPlayerNameUI(newName);

        nameInput.text = "";

        ToggleInputNamePanel(false);
    }

    public void CancelName() 
    {
        nameInput.text = "";

        ToggleInputNamePanel(false);
    }

    void ToggleInputNamePanel(bool toggle)
    {
        inputNameObj.SetActive(toggle);
    }
}
